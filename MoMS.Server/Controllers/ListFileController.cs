using MoMS.Server.Models.Dtos;
using MoMS.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace MoMS.Server.Controllers;

[ApiController]
public class ListFileController(ListFileService service) : ControllerBase
{
    [HttpGet("public/list/{fileName}")]
    public async Task<IActionResult> ReadListFile(string fileName, CancellationToken cancellationToken)
    {
        var content = await service.ReadAsync(fileName, cancellationToken);
        return content is null
            ? NotFound(new { error = $"Unable to read file: {fileName}" })
            : Content(content, "text/plain");
    }

    [HttpPost("api/save-file")]
    public async Task<IActionResult> SaveListFile(
        [FromBody] SaveFileDto dto,
        CancellationToken cancellationToken)
    {
        var saved = await service.SaveAsync(dto, cancellationToken);
        return saved ? Ok("File saved successfully") : BadRequest("Invalid file name.");
    }
}