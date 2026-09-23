using Microsoft.AspNetCore.Authorization;
using RMS.Application.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RMS.Api.Authorization;

internal sealed class AnyUserPermissionAuthorizationHandler(IUser userService)
    : AuthorizationHandler<AnyUserPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyUserPermissionRequirement requirement)
    {
        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(userIdValue, out var userId))
            return;

        foreach (var permission in requirement.Permissions)
        {
            if (await userService.HasAccessAsync(
                    userId,
                    permission.Section,
                    permission.Operation))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}