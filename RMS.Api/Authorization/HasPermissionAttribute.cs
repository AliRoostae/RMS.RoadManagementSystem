using Microsoft.AspNetCore.Authorization;
using RMS.Shared.Enums;

namespace RMS.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(UserSection section, UserAccessOperation operation)
    {
        Policy = PermissionPolicy.Create(section, operation);
    }
}
