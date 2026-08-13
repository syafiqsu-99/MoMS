using MoMS.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace MoMS.Server.Controllers;

[ApiController]
[Route("api")]
public class TimelineController(TimelineService service) : ControllerBase
{
    [HttpGet("timeline")]
    public async Task<IActionResult> GetTimeline(CancellationToken cancellationToken)
    {
        return Ok(await service.GetTimelineAsync(cancellationToken));
    }
}