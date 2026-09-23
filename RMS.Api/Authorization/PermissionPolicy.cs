using RMS.Shared.Enums;

namespace RMS.Api.Authorization;

internal static class PermissionPolicy
{
    private const string Prefix = "RMS.Permission";

    internal static string Create(UserSection section, UserAccessOperation operation) =>
        $"{Prefix}:{(byte)section}:{(byte)operation}";

    internal static bool TryParse(
        string policyName,
        out UserSection section,
        out UserAccessOperation operation)
    {
        section = default;
        operation = default;

        var parts = policyName.Split(':');
        return parts.Length == 3 &&
               string.Equals(parts[0], Prefix, StringComparison.Ordinal) &&
               byte.TryParse(parts[1], out var sectionValue) &&
               byte.TryParse(parts[2], out var operationValue) &&
               Enum.IsDefined(section = (UserSection)sectionValue) &&
               (operation = (UserAccessOperation)operationValue) != UserAccessOperation.None;
    }
}