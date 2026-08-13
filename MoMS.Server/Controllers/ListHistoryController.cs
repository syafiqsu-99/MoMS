using MoMS.Server.Models.Dtos;
using MoMS.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MoMS.Server.Controllers;

[ApiController]
[Route("api")]
public class ListHistoryController(ListHistoryService service) : ControllerBase
{
    [HttpGet("list-history")]
    public async Task<IActionResult> GetListHistory(CancellationToken cancellationToken)
    {
        return Ok(await service.GetAllAsync(cancellationToken));
    }

    [HttpPost("list-history")]
    public async Task<IActionResult> AddListHistory(
        [FromBody] JsonElement body,
        CancellationToken cancellationToken)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        if (body.ValueKind == JsonValueKind.Array)
        {
            var items = body.Deserialize<List<ListHistoryBatchItemDto>>(options) ?? [];
            await service.AddBatchAsync(items, cancellationToken);
            return Ok("All items added successfully.");
        }

        var single = body.Deserialize<ListHistorySingleDto>(options);
        if (single is null)
        {
            return BadRequest("Invalid list-history payload.");
        }

        await service.AddSingleAsync(single, cancellationToken);
        return Ok("Item added successfully.");
    }
}