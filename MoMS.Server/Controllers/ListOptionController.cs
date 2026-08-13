using MoMS.Server.Models.Dtos;
using MoMS.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace MoMS.Server.Controllers;

[ApiController]
[Route("api")]
public class ListOptionController(ListOptionService service) : ControllerBase
{
    [HttpGet("list-options")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await service.GetAllAsync(cancellationToken));
    }

    [HttpGet("list-options/{category}")]
    public async Task<IActionResult> GetByCategory(string category, CancellationToken cancellationToken)
    {
        var values = await service.GetByCategoryAsync(category, cancellationToken);
        return values is null ? BadRequest("Invalid category.") : Ok(values);
    }

    [HttpPost("list-options")]
    public async Task<IActionResult> Create(
        [FromBody] ListOptionCreateDto dto,
        CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(dto, cancellationToken);
        return created ? Ok("Option added successfully.") : BadRequest("Invalid or duplicate option.");
    }

    [HttpPut("list-options")]
    public async Task<IActionResult> Update(
        [FromBody] ListOptionUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(dto, cancellationToken);
        return updated ? Ok("Option updated successfully.") : NotFound("Option not found.");
    }

    [HttpDelete("list-options")]
    public async Task<IActionResult> Delete(
        [FromBody] ListOptionDeleteDto dto,
        CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(dto, cancellationToken);
        return deleted ? Ok("Option deleted successfully.") : NotFound("Option not found.");
    }
}