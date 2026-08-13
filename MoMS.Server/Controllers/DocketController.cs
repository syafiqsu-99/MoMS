using MoMS.Server.Models.Dtos;
using MoMS.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace MoMS.Server.Controllers;

[ApiController]
[Route("api")]
public class DocketController(DocketService service) : ControllerBase
{
    [HttpGet("dockets")]
    public async Task<IActionResult> GetDockets(CancellationToken cancellationToken)
    {
        return Ok(await service.GetAllAsync(cancellationToken));
    }

    [HttpPost("dockets")]
    public async Task<IActionResult> CreateDocket(
        [FromBody] DocketCreateDto dto,
        CancellationToken cancellationToken)
    {
        await service.CreateAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, "Docket saved successfully.");
    }

    // {pdfName} is the PDF_NAME primary key, matching the original :id semantics.
    [HttpDelete("dockets/{pdfName}")]
    public async Task<IActionResult> DeleteDocket(string pdfName, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(pdfName, cancellationToken);
        return deleted ? Ok("Docket deleted successfully.") : NotFound("Docket not found.");
    }

    [HttpGet("download/docket/{filename}")]
    public IActionResult DownloadDocket(string filename)
    {
        var path = service.ResolveDownloadPath(filename);
        if (path is null)
        {
            return NotFound("File not found");
        }

        var stream = System.IO.File.OpenRead(path);
        return File(stream, "application/pdf", filename);
    }
}