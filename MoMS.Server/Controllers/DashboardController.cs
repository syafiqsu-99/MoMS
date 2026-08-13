using MoMS.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace MoMS.Server.Controllers;

[ApiController]
[Route("api")]
public class DashboardController(DashboardService service) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var status = await service.GetStatusAsync(cancellationToken);
        return Ok(status);
    }

    [HttpGet("machines")]
    public async Task<IActionResult> GetMachines(CancellationToken cancellationToken)
    {
        return Ok(await service.GetMachinesAsync(cancellationToken));
    }

    [HttpGet("loadMachineMaster")]
    public async Task<IActionResult> LoadMachineMaster(CancellationToken cancellationToken)
    {
        return Ok(await service.GetMachineMasterAsync(cancellationToken));
    }
}