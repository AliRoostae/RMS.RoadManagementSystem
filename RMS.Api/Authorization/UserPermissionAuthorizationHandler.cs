using Microsoft.AspNetCore.Authorization;
using RMS.Application.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RMS.Api.Authorization;

internal sealed class UserPermissionAuthorizationHandler(IUser userService)
    : AuthorizationHandler<UserPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserPermissionRequirement requirement)
    {
        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(userIdValue, out var userId))
            return;

        if (await userService.HasAccessAsync(
                userId,
                requirement.Section,
                requirement.Operation))
        {
            context.Succeed(requirement);
        }
    }
}
