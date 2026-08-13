using MoMS.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace MoMS.Server.Controllers;

[ApiController]
[Route("api")]
public class PreparationController(PreparationService service) : ControllerBase
{
    [HttpGet("preparation")]
    public async Task<IActionResult> GetPreparation(CancellationToken cancellationToken)
    {
        return Ok(await service.GetPreparationAsync(cancellationToken));
    }

    // GET /api/sharing-product?type=... — type maps to the proc's @Type param.
    [HttpGet("sharing-product")]
    public async Task<IActionResult> GetSharingProduct(
        [FromQuery] string? type,
        CancellationToken cancellationToken)
    {
        return Ok(await service.GetSharingProductAsync(type, cancellationToken));
    }
}