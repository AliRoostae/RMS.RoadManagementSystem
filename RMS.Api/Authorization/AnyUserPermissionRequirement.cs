using Microsoft.AspNetCore.Authorization;

namespace RMS.Api.Authorization;

internal sealed record AnyUserPermissionRequirement(
    IReadOnlyList<UserPermissionRequirement> Permissions) : IAuthorizationRequirement;