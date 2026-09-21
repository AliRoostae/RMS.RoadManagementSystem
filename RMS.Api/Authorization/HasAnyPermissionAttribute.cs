using Microsoft.AspNetCore.Authorization;
using RMS.Shared.Enums;

namespace RMS.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class HasAnyPermissionAttribute : AuthorizeAttribute
{
    public HasAnyPermissionAttribute(
        UserSection section1,
        UserAccessOperation operation1,
        UserSection section2,
        UserAccessOperation operation2,
        UserSection section3 = 0,
        UserAccessOperation operation3 = UserAccessOperation.None,
        UserSection section4 = 0,
        UserAccessOperation operation4 = UserAccessOperation.None)
    {
        var permissions = new List<UserPermissionRequirement>
        {
            new(section1, operation1),
            new(section2, operation2)
        };
        AddIfValid(permissions, section3, operation3);
        AddIfValid(permissions, section4, operation4);
        Policy = AnyPermissionPolicy.Create(permissions);
    }

    private static void AddIfValid(
        ICollection<UserPermissionRequirement> permissions,
        UserSection section,
        UserAccessOperation operation)
    {
        if (section != 0 && operation != UserAccessOperation.None)
            permissions.Add(new UserPermissionRequirement(section, operation));
    }
}
