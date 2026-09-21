using Microsoft.AspNetCore.Mvc;
using RMS.Api.Authorization;
using RMS.Application.Interface;
using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.Responses;
using RMS.Shared.Enums;

namespace RMS.Api.Controllers;

[ApiController]
[Route("api/images")]
public sealed class ImagesController(IImageStorageService service) : ControllerBase
{
    [HttpPost]
    [HasPermission(UserSection.Images, UserAccessOperation.Create)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAccidentImageCommand command,
        CancellationToken token)
    {
        var id = await service.AddImageUrlAsync(command, token);
        return id == Guid.Empty
            ? Problem(statusCode: StatusCodes.Status500InternalServerError, title: "تصویر ذخیره نشد.")
            : CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(UserSection.Images, UserAccessOperation.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await service.DeleteAsync(new DeleteAccidentImageCommand(id), token);
        return NoContent();
    }

    [HttpDelete("accident/{accidentId:guid}")]
    [HasPermission(UserSection.Images, UserAccessOperation.Delete)]
    public async Task<IActionResult> DeleteAll(Guid accidentId, CancellationToken token)
    {
        await service.DeleteAllAsync(new DeleteAccidentImagesCommand(accidentId), token);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    [HasPermission(UserSection.Images, UserAccessOperation.View)]
    [ProducesResponseType<ImageResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        var result = await service.GetAsync(id, token);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("accident/{accidentId:guid}")]
    [HasPermission(UserSection.Images, UserAccessOperation.Report)]
    public async Task<ActionResult<IReadOnlyList<ImageResponse>>> GetAll(
        Guid accidentId,
        CancellationToken token) =>
        Ok(await service.GetAllAsync(accidentId, token));
}
