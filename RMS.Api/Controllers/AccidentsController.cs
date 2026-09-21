using Microsoft.AspNetCore.Mvc;
using RMS.Api.Authorization;
using RMS.Application.Interface;
using RMS.Shared.Contracts.Commands;
using RMS.Shared.Enums;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;

namespace RMS.Api.Controllers;

/// <summary>endpointهای ثبت، ویرایش، حذف و گزارش‌گیری حوادث.</summary>
[ApiController]
[Route("api/accidents")]
public sealed class AccidentsController(IAccident service) : ControllerBase
{
    /// <summary>یک حادثهٔ جدید ثبت می‌کند.</summary>
    [HttpPost]
    [HasPermission(UserSection.Accidents, UserAccessOperation.Create)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAccidentCommand command,
        CancellationToken token)
    {
        var id = await service.AddAsync(command, token);
        return id == Guid.Empty
            ? Problem(statusCode: StatusCodes.Status500InternalServerError, title: "حادثه ذخیره نشد.")
            : CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>اطلاعات حادثه را ویرایش می‌کند.</summary>
    [HttpPut("{id:guid}")]
    [HasPermission(UserSection.Accidents, UserAccessOperation.Update)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAccidentCommand command,
        CancellationToken token)
    {
        command.Id = id;
        return await service.UpdateAsync(command, token) ? NoContent() : NotFound();
    }

    /// <summary>حادثه را حذف می‌کند.</summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(UserSection.Accidents, UserAccessOperation.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token) =>
        await service.DeleteAsync(new DeleteAccidentCommand(id), token)
            ? NoContent()
            : NotFound();

    /// <summary>جزئیات یک حادثه را برمی‌گرداند.</summary>
    [HttpGet("{id:guid}")]
    [HasPermission(UserSection.Accidents, UserAccessOperation.View)]
    [ProducesResponseType<AccidentResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        var result = await service.GetAsync(id, token);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>فهرست صفحه‌بندی‌شدهٔ حوادث را برمی‌گرداند.</summary>
    [HttpGet]
    [HasPermission(UserSection.Accidents, UserAccessOperation.Report)]
    public async Task<ActionResult<PagedResponse<AccidentListItemResponse>>> GetAll(
        [FromQuery] AccidentQuery query,
        CancellationToken token) =>
        Ok(await service.GetAllAsync(query, token));

    /// <summary>نقاط حوادث را برای نمایش روی نقشه برمی‌گرداند.</summary>
    [HttpGet("map")]
    [HasPermission(UserSection.Accidents, UserAccessOperation.Report)]
    public async Task<ActionResult<IReadOnlyList<AccidentMapPointResponse>>> GetMap(
        [FromQuery] AccidentMapQuery query,
        CancellationToken token) =>
        Ok(await service.GetMapAsync(query, token));
}
