using MoMS.Server.Models.Dtos;
using MoMS.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace MoMS.Server.Controllers;

[ApiController]
[Route("api")]
public class FullListRepeatController(FullListRepeatService service) : ControllerBase
{
    [HttpPost("update-repeat")]
    public async Task<IActionResult> UpdateRepeat(
        [FromBody] UpdateRepeatDto dto,
        CancellationToken cancellationToken)
    {
        await service.UpdateRepeatAsync(dto, cancellationToken);
        return Ok("Data updated successfully.");
    }
}