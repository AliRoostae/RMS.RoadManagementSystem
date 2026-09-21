using Microsoft.AspNetCore.Authorization;
using RMS.Shared.Enums;

namespace RMS.Api.Authorization;

internal sealed record UserPermissionRequirement(
    UserSection Section,
    UserAccessOperation Operation) : IAuthorizationRequirement;
