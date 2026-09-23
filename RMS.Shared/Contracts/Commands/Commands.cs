using RMS.Shared.Contracts.DTOs;
using RMS.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace RMS.Shared.Contracts.Commands;

/// <summary>فرمان ثبت حادثهٔ جدید.</summary>
public sealed class CreateAccidentCommand : BaseAccident;
/// <summary>فرمان ویرایش حادثهٔ موجود.</summary>
public sealed class UpdateAccidentCommand : BaseAccident { public Guid Id { get; set; } }
/// <summary>فرمان حذف حادثه.</summary>
public sealed record DeleteAccidentCommand(Guid Id);

/// <summary>فرمان ثبت خودرو برای یک حادثه.</summary>
public sealed class CreateCarCommand : BaseCar;
/// <summary>فرمان ویرایش خودرو.</summary>
public sealed class UpdateCarCommand : BaseCarEdit { public Guid Id { get; set; } }
/// <summary>فرمان حذف خودرو.</summary>
public sealed record DeleteCarCommand(Guid Id);

/// <summary>فرمان ثبت مسافر.</summary>
public sealed class CreatePassengerCommand : BasePassenger;
/// <summary>فرمان ویرایش مسافر.</summary>
public sealed class UpdatePassengerCommand : SharedPassengerPeople
{
    /// <summary>شناسهٔ مسافر.</summary>
    public Guid Id { get; set; }
    /// <summary>مشخص می‌کند مسافر، رانندهٔ خودرو هست یا نه.</summary>
    public bool IsDriver { get; set; }
}
/// <summary>فرمان حذف مسافر.</summary>
public sealed record DeletePassengerCommand(Guid Id);

/// <summary>فرمان ثبت فرد خارج از خودرو در حادثه.</summary>
public sealed class CreatePeopleCommand : BasePeople;
/// <summary>فرمان ویرایش فرد خارج از خودرو.</summary>
public sealed class UpdatePeopleCommand : SharedPassengerPeople { public Guid Id { get; set; } }
/// <summary>فرمان حذف فرد.</summary>
public sealed record DeletePeopleCommand(Guid Id);

/// <summary>فرمان ثبت راه یا محدودهٔ جغرافیایی.</summary>
public sealed class CreateRoadCommand : BaseRoads;
/// <summary>فرمان ویرایش راه یا محدودهٔ جغرافیایی.</summary>
public sealed class UpdateRoadCommand : BaseRoads { public Guid Id { get; set; } }
/// <summary>فرمان حذف راه.</summary>
public sealed record DeleteRoadCommand(Guid Id);

/// <summary>فرمان افزودن تصویر برای یک حادثه.</summary>
public sealed class CreateAccidentImageCommand
{
    /// <summary>شناسهٔ حادثهٔ مربوط به تصویر.</summary>
    public Guid AccidentId { get; set; }
    [Required]
    /// <summary>محتوای تصویر به‌صورت Base64.</summary>
    public string Base64Image { get; set; } = string.Empty;
}
/// <summary>فرمان حذف یک تصویر حادثه.</summary>
public sealed record DeleteAccidentImageCommand(Guid ImageId);
/// <summary>فرمان حذف همهٔ تصاویر یک حادثه.</summary>
public sealed record DeleteAccidentImagesCommand(Guid AccidentId);

/// <summary>مجوزهای قابل ثبت برای یک بخش از سامانه.</summary>
public sealed class UserPermissionCommand
{
    [EnumDataType(typeof(UserSection))]
    /// <summary>بخشی که مجوز روی آن اعمال می‌شود.</summary>
    public UserSection Section { get; set; }
    /// <summary>عملیات مجاز در آن بخش.</summary>
    public UserAccessOperation Operations { get; set; }
}

/// <summary>فرمان ساخت کاربر جدید.</summary>
public sealed class CreateUserCommand
{
    [Required, MaxLength(50)]
    /// <summary>نام کاربر.</summary>
    public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(50)]
    /// <summary>نام خانوادگی کاربر.</summary>
    public string LastName { get; set; } = string.Empty;
    [Required, MaxLength(20)]
    /// <summary>شماره موبایل کاربر.</summary>
    public string MobileNumber { get; set; } = string.Empty;
    [Required, MinLength(6), MaxLength(100)]
    /// <summary>رمز عبور اولیه.</summary>
    public string Password { get; set; } = string.Empty;
    [EnumDataType(typeof(UserRole))]
    /// <summary>نقش کلی کاربر.</summary>
    public UserRole Role { get; set; } = UserRole.User;
    [Required]
    /// <summary>فهرست مجوزهای کاربر.</summary>
    public IReadOnlyCollection<UserPermissionCommand> Permissions { get; set; } = [];
}

/// <summary>فرمان ویرایش اطلاعات و مجوزهای کاربر.</summary>
public sealed class UpdateUserCommand
{
    /// <summary>شناسهٔ کاربر.</summary>
    public Guid Id { get; set; }
    [Required, MaxLength(50)]
    /// <summary>نام کاربر.</summary>
    public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(50)]
    /// <summary>نام خانوادگی کاربر.</summary>
    public string LastName { get; set; } = string.Empty;
    [Required, MaxLength(20)]
    /// <summary>شماره موبایل کاربر.</summary>
    public string MobileNumber { get; set; } = string.Empty;
    [MinLength(6), MaxLength(100)]
    /// <summary>رمز جدید؛ اگر خالی باشد رمز قبلی حفظ می‌شود.</summary>
    public string? Password { get; set; }
    [EnumDataType(typeof(UserRole))]
    /// <summary>نقش کلی کاربر.</summary>
    public UserRole Role { get; set; } = UserRole.User;
    [Required]
    /// <summary>فهرست مجوزهای جدید کاربر.</summary>
    public IReadOnlyCollection<UserPermissionCommand> Permissions { get; set; } = [];
}
/// <summary>فرمان حذف کاربر.</summary>
public sealed record DeleteUserCommand(Guid Id);