using Microsoft.AspNetCore.Mvc;
using RMS.Api.Authorization;
using RMS.Application.Interface;
using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;
using RMS.Shared.Enums;

namespace RMS.Api.Controllers;

[ApiController]
[Route("api/cars")]
public sealed class CarsController(ICar service) : ControllerBase
{
    [HttpPost]
    [HasPermission(UserSection.Cars, UserAccessOperation.Create)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCarCommand command,
        CancellationToken token)
    {
        var id = await service.AddAsync(command, token);
        return id == Guid.Empty
            ? Problem(statusCode: StatusCodes.Status500InternalServerError, title: "خودرو ذخیره نشد.")
            : CreatedAtAction(nameof(GetById), new
            {
                id
            }, new
            {
                id
            });
    }

    [HttpPut("{id:guid}")]
    [HasPermission(UserSection.Cars, UserAccessOperation.Update)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCarCommand command,
        CancellationToken token)
    {
        command.Id = id;
        return await service.UpdateAsync(command, token) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(UserSection.Cars, UserAccessOperation.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token) =>
        await service.DeleteAsync(new DeleteCarCommand(id), token)
            ? NoContent()
            : NotFound();

    [HttpGet("{id:guid}")]
    [HasPermission(UserSection.Cars, UserAccessOperation.View)]
    [ProducesResponseType<CarResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        var result = await service.GetAsync(id, token);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [HasPermission(UserSection.Cars, UserAccessOperation.Report)]
    public async Task<ActionResult<PagedResponse<CarListItemResponse>>> GetAll(
        [FromQuery] CarQueries query,
        CancellationToken token) =>
        Ok(await service.GetAllAsync(query, token));
}