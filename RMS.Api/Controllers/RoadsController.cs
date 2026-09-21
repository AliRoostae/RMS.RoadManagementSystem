using Microsoft.AspNetCore.Mvc;
using RMS.Api.Authorization;
using RMS.Application.Interface;
using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;
using RMS.Shared.Enums;

namespace RMS.Api.Controllers;

[ApiController]
[Route("api/roads")]
public sealed class RoadsController(IRoad service) : ControllerBase
{
    [HttpPost]
    [HasPermission(UserSection.Roads, UserAccessOperation.Create)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoadCommand command,
        CancellationToken token)
    {
        var id = await service.AddAsync(command, token);
        return id == Guid.Empty
            ? Problem(statusCode: StatusCodes.Status500InternalServerError, title: "راه ذخیره نشد.")
            : CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [HasPermission(UserSection.Roads, UserAccessOperation.Update)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateRoadCommand command,
        CancellationToken token)
    {
        command.Id = id;
        return await service.UpdateAsync(command, token) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(UserSection.Roads, UserAccessOperation.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token) =>
        await service.DeleteAsync(new DeleteRoadCommand(id), token)
            ? NoContent()
            : NotFound();

    [HttpGet("{id:guid}")]
    [HasPermission(UserSection.Roads, UserAccessOperation.View)]
    [ProducesResponseType<RoadResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        var result = await service.GetAsync(id, token);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/geometry")]
    [HasAnyPermission(
        UserSection.Roads, UserAccessOperation.View,
        UserSection.Accidents, UserAccessOperation.Create,
        UserSection.Accidents, UserAccessOperation.Update)]
    [ProducesResponseType<RoadGeometryResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGeometry(Guid id, CancellationToken token)
    {
        var result = await service.GetGeometryAsync(id, token);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/nearest-point")]
    [HasAnyPermission(
        UserSection.Roads, UserAccessOperation.View,
        UserSection.Accidents, UserAccessOperation.Create,
        UserSection.Accidents, UserAccessOperation.Update)]
    [ProducesResponseType<RoadLocationResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNearestPoint(
        Guid id,
        [FromQuery] RoadLocationQuery query,
        CancellationToken token)
    {
        var result = await service.GetNearestPointAsync(id, query, token);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [HasPermission(UserSection.Roads, UserAccessOperation.Report)]
    public async Task<ActionResult<PagedResponse<RoadListItemResponse>>> GetAll(
        [FromQuery] RoadQueries query,
        CancellationToken token) =>
        Ok(await service.GetAllAsync(query, token));

    [HttpGet("map")]
    [HasPermission(UserSection.Roads, UserAccessOperation.Report)]
    public async Task<ActionResult<IReadOnlyList<RoadMapItemResponse>>> GetMap(
        [FromQuery] RoadMapQuery query,
        CancellationToken token) =>
        Ok(await service.GetMapAsync(query, token));
}
