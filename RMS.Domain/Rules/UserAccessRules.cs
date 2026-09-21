using RMS.Shared.Enums;
using RMS.Domain.Interfaces;
using RMS.Domain.Security;
using System.ComponentModel.DataAnnotations;

namespace RMS.Domain.Rules;

/// <summary>
/// قوانین مرکزی دسترسی کاربران. Policyهای API می‌توانند از طریق IUser
/// نتیجهٔ این کلاس را دریافت کنند و قواعد جدید نیز با ارث‌بری یا جایگزینی اینترفیس افزوده شوند.
/// </summary>
public class UserAccessRules : IUserAccessRules
{
    private const UserAccessOperation AllOperations =
        UserAccessOperation.View |
        UserAccessOperation.Create |
        UserAccessOperation.Update |
        UserAccessOperation.Delete |
        UserAccessOperation.Report;

    /// <summary>مجوزها را از نظر تکراری نبودن بخش و معتبر بودن عملیات بررسی می‌کند.</summary>
    /// <param name="permissions">مجوزهای پیشنهادی کاربر.</param>
    public virtual void EnsurePermissionsAreValid(IEnumerable<UserPermissionValue> permissions)
    {
        ArgumentNullException.ThrowIfNull(permissions);

        var permissionList = permissions.ToArray();
        if (permissionList.GroupBy(item => item.Section).Any(group => group.Count() > 1))
            throw new ValidationException("برای هر بخش فقط یک دسترسی قابل ثبت است.");

        foreach (var permission in permissionList)
        {
            if (!Enum.IsDefined(permission.Section))
                throw new ValidationException("بخش دسترسی نامعتبر است.");

            if (permission.Operations == UserAccessOperation.None ||
                (permission.Operations & ~AllOperations) != UserAccessOperation.None)
            {
                throw new ValidationException("عملیات دسترسی نامعتبر است.");
            }
        }
    }

    /// <summary>بررسی می‌کند کاربر برای عملیات مشخص‌شده دسترسی دارد یا نه.</summary>
    /// <param name="user">تصویر فعلی دسترسی کاربر.</param>
    /// <param name="section">بخش موردنظر.</param>
    /// <param name="operation">عملیات موردنظر.</param>
    /// <returns>اگر دسترسی برقرار باشد <see langword="true"/>؛ وگرنه <see langword="false"/>.</returns>
    public virtual bool HasAccess(
        UserAccessSnapshot user,
        UserSection section,
        UserAccessOperation operation)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!Enum.IsDefined(section) ||
            operation == UserAccessOperation.None ||
            (operation & ~AllOperations) != UserAccessOperation.None)
        {
            return false;
        }

        if (HasRoleOverride(user.Role, section, operation))
            return true;

        var grantedOperations = user.Permissions
            .Where(permission => permission.Section == section)
            .Aggregate(
                UserAccessOperation.None,
                (current, permission) => current | permission.Operations);

        return (grantedOperations & operation) == operation;
    }

    /// <summary>نقطهٔ توسعه برای نقش‌هایی که مستقل از مجوزهای صریح دسترسی دارند.</summary>
    protected virtual bool HasRoleOverride(
        UserRole role,
        UserSection section,
        UserAccessOperation operation) => role == UserRole.Administrator;
}
