using RMS.Domain.Security;
using RMS.Shared.Enums;

namespace RMS.Domain.Interfaces;

/// <summary>قوانین قابل‌جایگزینی و قابل‌گسترش دسترسی کاربران را تعریف می‌کند.</summary>
public interface IUserAccessRules
{
    /// <summary>مجوزهای کاربر را اعتبارسنجی می‌کند و در صورت ایراد خطا می‌دهد.</summary>
    /// <param name="permissions">مجوزهای قابل بررسی.</param>
    void EnsurePermissionsAreValid(IEnumerable<UserPermissionValue> permissions);

    /// <summary>بررسی می‌کند یک عملیات برای کاربر مجاز هست یا نه.</summary>
    /// <param name="user">اطلاعات دسترسی کاربر.</param>
    /// <param name="section">بخش سامانه.</param>
    /// <param name="operation">عملیات موردنظر.</param>
    /// <returns>نتیجهٔ بررسی دسترسی.</returns>
    bool HasAccess(
        UserAccessSnapshot user,
        UserSection section,
        UserAccessOperation operation);
}