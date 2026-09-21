using RMS.Shared.Enums;

namespace RMS.Api.Authorization;

internal static class AnyPermissionPolicy
{
    private const string Prefix = "RMS.AnyPermission";

    internal static string Create(IEnumerable<UserPermissionRequirement> permissions) =>
        $"{Prefix}:{string.Join('|', permissions.Select(permission =>
            $"{(byte)permission.Section},{(byte)permission.Operation}"))}";

    internal static bool TryParse(
        string policyName,
        out IReadOnlyList<UserPermissionRequirement> permissions)
    {
        permissions = [];
        var separator = policyName.IndexOf(':');
        if (separator < 0 || !string.Equals(policyName[..separator], Prefix, StringComparison.Ordinal))
            return false;

        var parsed = new List<UserPermissionRequirement>();
        foreach (var item in policyName[(separator + 1)..].Split('|', StringSplitOptions.RemoveEmptyEntries))
        {
            var values = item.Split(',');
            if (values.Length != 2 ||
                !byte.TryParse(values[0], out var sectionValue) ||
                !byte.TryParse(values[1], out var operationValue) ||
                !Enum.IsDefined((UserSection)sectionValue) ||
                (UserAccessOperation)operationValue == UserAccessOperation.None)
            {
                permissions = [];
                return false;
            }

            parsed.Add(new UserPermissionRequirement(
                (UserSection)sectionValue,
                (UserAccessOperation)operationValue));
        }

        permissions = parsed;
        return parsed.Count > 0;
    }
}
