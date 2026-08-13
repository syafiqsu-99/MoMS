using MoMS.Server.Models;
using MoMS.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace MoMS.Server.Controllers;

[ApiController]
[Route("api")]
public class FullListController(FullListService service) : ControllerBase
{
    [HttpGet("full-list")]
    public async Task<ActionResult<IEnumerable<FullList>>> GetFullList(CancellationToken cancellationToken)
    {
        return Ok(await service.GetAllAsync(cancellationToken));
    }

    [HttpGet("locations")]
    public async Task<ActionResult<IEnumerable<Location>>> GetLocations(CancellationToken cancellationToken)
    {
        return Ok(await service.GetLocationsAsync(cancellationToken));
    }

    [HttpPost("full-list")]
    public async Task<IActionResult> CreateFullList(
        [FromBody] FullListCreateDto dto,
        CancellationToken cancellationToken)
    {
        await service.CreateAsync(dto, cancellationToken);
        return Ok("full list inserted successfully");
    }

    [HttpPut("full-list/{sNum}")]
    public async Task<IActionResult> UpdateFullList(
        string sNum,
        [FromBody] FullListUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(sNum, dto, cancellationToken);
        return updated ? Ok("full list item updated successfully") : NotFound();
    }

    [HttpDelete("full-list/{sNum}")]
    public async Task<IActionResult> DeleteFullList(string sNum, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(sNum, cancellationToken);
        return deleted ? Ok("full list item deleted successfully") : NotFound();
    }
}
