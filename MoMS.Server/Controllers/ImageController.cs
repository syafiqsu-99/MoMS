using MoMS.Server.Models.Dtos;
using MoMS.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace MoMS.Server.Controllers;

[ApiController]
[Route("api")]
public class ImageController(ImageStorageService service) : ControllerBase
{
    // multipart/form-data field name "images[]" preserved from the client.
    [HttpPost("upload-images-server")]
    public async Task<IActionResult> UploadImages(CancellationToken cancellationToken)
    {
        var files = Request.Form.Files;
        if (files.Count == 0)
        {
            return BadRequest("No files uploaded.");
        }

        await service.SaveUploadedAsync(files, cancellationToken);
        return Ok("Images saved successfully.");
    }

    [HttpPost("upload-images-sql")]
    public async Task<IActionResult> StoreImageNames(
        [FromBody] UploadImagesSqlDto dto,
        CancellationToken cancellationToken)
    {
        await service.StoreImageNamesAsync(dto, cancellationToken);
        return Ok("Image names stored in SQL database successfully.");
    }

    [HttpPut("upload-images")]
    public async Task<IActionResult> DeleteImages(
        [FromBody] DeleteImagesDto dto,
        CancellationToken cancellationToken)
    {
        await service.DeleteImagesAsync(dto, cancellationToken);
        return Ok("Images deleted successfully.");
    }
}