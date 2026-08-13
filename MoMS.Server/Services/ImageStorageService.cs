using MoMS.Server.Data;
using MoMS.Server.Models.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MoMS.Server.Services;

public class ImageStorageService(MoMsDbContext context, IWebHostEnvironment environment)
    : BaseService(context)
{
    private string ToolsetImageDirectory =>
        Path.Combine(environment.WebRootPath, "static", "toolset_img");

    public async Task<int> SaveUploadedAsync(
        IFormFileCollection files,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(ToolsetImageDirectory);

        var saved = 0;
        foreach (var file in files)
        {
            if (file.Length == 0)
            {
                continue;
            }

            var safeName = Path.GetFileName(file.FileName);
            var destination = Path.Combine(ToolsetImageDirectory, safeName);

            await using var stream = new FileStream(destination, FileMode.Create);
            await file.CopyToAsync(stream, cancellationToken);
            saved++;
        }

        return saved;
    }

    public async Task StoreImageNamesAsync(UploadImagesSqlDto dto, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE list_history SET IMG_NAME = @imgName
            WHERE ITEM = @item AND S_NUM = @sNum AND [FROM] = @from
              AND [TO] = @to AND DATETIME = @dateTime AND STATUS = @status";

        var parameters = new[]
        {
            new SqlParameter("@imgName", dto.ImgName),
            new SqlParameter("@item", dto.Item),
            new SqlParameter("@sNum", dto.SNum),
            new SqlParameter("@from", dto.From),
            new SqlParameter("@to", dto.To),
            new SqlParameter("@dateTime", dto.DateTime),
            new SqlParameter("@status", dto.Status)
        };

        await Context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }

    public async Task DeleteImagesAsync(DeleteImagesDto dto, CancellationToken cancellationToken)
    {
        var names = dto.ImgName
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var name in names)
        {
            var path = Path.Combine(ToolsetImageDirectory, Path.GetFileName(name));
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        const string sql = @"
            UPDATE list_history SET IMG_NAME = NULL
            WHERE ITEM = @item AND S_NUM = @sNum AND [FROM] = @from
              AND [TO] = @to AND DATETIME = @dateTime AND STATUS = @status";

        var parameters = new[]
        {
            new SqlParameter("@item", dto.Item),
            new SqlParameter("@sNum", dto.SNum),
            new SqlParameter("@from", dto.From),
            new SqlParameter("@to", dto.To),
            new SqlParameter("@dateTime", dto.DateTime),
            new SqlParameter("@status", dto.Status)
        };

        await Context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }
}