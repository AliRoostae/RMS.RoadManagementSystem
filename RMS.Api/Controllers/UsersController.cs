using Microsoft.AspNetCore.Mvc;
using RMS.Api.Authorization;
using RMS.Application.Interface;
using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;
using RMS.Shared.Enums;

namespace RMS.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(IUser service) : ControllerBase
{
    [HttpPost]
    [HasPermission(UserSection.Users, UserAccessOperation.Create)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserCommand command,
        CancellationToken token)
    {
        var id = await service.AddAsync(command, token);
        return id == Guid.Empty
            ? Problem(statusCode: StatusCodes.Status500InternalServerError, title: "کاربر ذخیره نشد.")
            : CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [HasPermission(UserSection.Users, UserAccessOperation.Update)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserCommand command,
        CancellationToken token)
    {
        command.Id = id;
        return await service.UpdateAsync(command, token) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(UserSection.Users, UserAccessOperation.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token) =>
        await service.DeleteAsync(new DeleteUserCommand(id), token)
            ? NoContent()
            : NotFound();

    [HttpGet("{id:guid}")]
    [HasPermission(UserSection.Users, UserAccessOperation.View)]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        var result = await service.GetAsync(id, token);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [HasPermission(UserSection.Users, UserAccessOperation.Report)]
    public async Task<ActionResult<PagedResponse<UserListItemResponse>>> GetAll(
        [FromQuery] UserQuery query,
        CancellationToken token) =>
        Ok(await service.GetAllAsync(query, token));
}