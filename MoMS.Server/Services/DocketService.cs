using MoMS.Server.Data;
using MoMS.Server.Models.Dtos;
using MoMS.Server.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MoMS.Server.Services;

public class DocketService(MoMsDbContext context, IWebHostEnvironment environment)
    : BaseService(context)
{
    private string DocketDirectory =>
        Path.Combine(environment.WebRootPath, "static", "docket_pdf");

    private string ToolsetImageDirectory =>
        Path.Combine(environment.WebRootPath, "static", "toolset_img");

    public async Task<List<ListDocket>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await Context.ListDockets
            .AsNoTracking()
            .OrderByDescending(d => d.DateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<string> CreateAsync(DocketCreateDto dto, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(DocketDirectory);

        var year = dto.DateTime.Year;
        var today = DateTime.Today.ToString("yyyy-MM-dd");

        var existingForYear = Directory
            .EnumerateFiles(DocketDirectory)
            .Select(Path.GetFileName)
            .Count(name => name is not null && name.StartsWith($"{year}-", StringComparison.Ordinal));

        var sequence = existingForYear + 1;
        var pdfName = $"{today}({sequence}).pdf";
        var pdfPath = Path.Combine(DocketDirectory, pdfName);

        const string sql = @"
            INSERT INTO list_docket (ID, ITEM, S_NUM, PDF_NAME, VENDOR, DATETIME, YEAR_CREATED)
            VALUES (@id, @item, @sNum, @pdfName, @vendor, @dateTime, @year);

            UPDATE full_list SET REMARK = @remark WHERE S_NUM = @sNum;";

        var parameters = new[]
        {
            new SqlParameter("@id", sequence),
            new SqlParameter("@item", dto.Item),
            new SqlParameter("@sNum", dto.SNum),
            new SqlParameter("@pdfName", pdfName),
            new SqlParameter("@vendor", dto.Vendor),
            new SqlParameter("@dateTime", dto.DateTime),
            new SqlParameter("@year", year),
            new SqlParameter("@remark", (object?)dto.RemarksDetails ?? DBNull.Value)
        };

        await Context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);

        var images = LoadDocketImages(dto.Images);
        var logo = LoadLogo();

        BuildDocument(dto, images, logo).GeneratePdf(pdfPath);

        return pdfName;
    }

    public async Task<bool> DeleteAsync(string pdfName, CancellationToken cancellationToken)
    {
        var exists = await Context.ListDockets
            .AnyAsync(d => d.PdfName == pdfName, cancellationToken);

        if (!exists)
        {
            return false;
        }

        var path = Path.Combine(DocketDirectory, Path.GetFileName(pdfName));
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        await Context.ListDockets
            .Where(d => d.PdfName == pdfName)
            .ExecuteDeleteAsync(cancellationToken);

        return true;
    }

    // Resolves the absolute path for the download endpoint, or null if missing.
    public string? ResolveDownloadPath(string fileName)
    {
        var path = Path.Combine(DocketDirectory, Path.GetFileName(fileName));
        return File.Exists(path) ? path : null;
    }

    private List<byte[]> LoadDocketImages(IEnumerable<string> names)
    {
        var images = new List<byte[]>();
        foreach (var name in names)
        {
            var path = Path.Combine(ToolsetImageDirectory, Path.GetFileName(name));
            if (File.Exists(path))
            {
                images.Add(File.ReadAllBytes(path));
            }
        }
        return images;
    }

    private byte[]? LoadLogo()
    {
        var logoPath = Path.Combine(environment.WebRootPath, "assets", "JJfullblue.png");
        return File.Exists(logoPath) ? File.ReadAllBytes(logoPath) : null;
    }

    private static Document BuildDocument(DocketCreateDto dto, List<byte[]> images, byte[]? logo)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    if (logo is not null)
                    {
                        column.Item().AlignCenter().Width(150).Image(logo);
                    }

                    column.Item().AlignCenter().Text("Toolset's Docket - Revision 1")
                        .FontSize(22).Bold();
                    column.Item().AlignCenter()
                        .Text("To record parts, moulds & toolset sent out from JJPM-SB")
                        .FontSize(16);

                    AddField(column, "1. Vendor Company Name", dto.VendorName);
                    AddField(column, "2. Vendor PIC Name", dto.PicName);
                    AddField(column, "3. Date OUT", dto.DateOut);
                    AddField(column, "4. Time OUT", dto.TimeOut);
                    AddField(column, "5. Target Date IN", dto.DateIn);
                    AddField(column, "6. Purpose for Toolset Send Out", dto.SelectPurpose);
                    AddField(column, "7. Details (Model)", dto.ModelDetails);
                    AddField(column, "8. Details (Parts)", dto.PartsDetails);
                    AddField(column, "9. Details (Remarks)", dto.RemarksDetails);
                    AddField(column, "10. Docket Prepared By", dto.SelectPrepared);

                    column.Item().PaddingTop(10)
                        .Text("11. Photo & Evidence (Including Car Plate & Toolset)").Bold();

                    foreach (var image in images)
                    {
                        column.Item().PaddingVertical(10).Width(150).Image(image);
                    }
                });
            });
        });
    }

    private static void AddField(ColumnDescriptor column, string label, string? value)
    {
        column.Item().PaddingTop(10).Text(label).Bold();
        column.Item().PaddingLeft(10).Text(value ?? string.Empty);
    }
}