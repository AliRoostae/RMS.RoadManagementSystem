using RMS.Shared.Enums;

namespace RMS.Domain.Security;

/// <summary>مقدار مستقل از دیتابیس برای یک مجوز کاربر.</summary>
public sealed record UserPermissionValue(UserSection Section, UserAccessOperation Operations);

/// <summary>اطلاعات حداقلی لازم برای تصمیم‌گیری دسترسی.</summary>
public sealed record UserAccessSnapshot(
    Guid UserId,
    UserRole Role,
    IReadOnlyCollection<UserPermissionValue> Permissions);
