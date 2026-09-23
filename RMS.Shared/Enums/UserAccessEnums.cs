namespace RMS.Shared.Enums;

/// <summary>نقش کلی کاربر در سامانه.</summary>
public enum UserRole : byte
{
    User = 1,
    Manager = 2,
    Administrator = 3
}

/// <summary>بخش‌های قابل کنترل سامانه برای تعریف دسترسی کاربر.</summary>
public enum UserSection : byte
{
    Users = 1,
    Roads = 2,
    Accidents = 3,
    Cars = 4,
    Passengers = 5,
    People = 6,
    Images = 7,
    Reports = 8
}

/// <summary>عملیات قابل اعطا روی هر بخش سامانه.</summary>
[Flags]
public enum UserAccessOperation : byte
{
    None = 0,
    View = 1,
    Create = 2,
    Update = 4,
    Delete = 8,
    Report = 16
}