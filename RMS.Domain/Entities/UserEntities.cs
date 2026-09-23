using System.ComponentModel.DataAnnotations;
using RMS.Shared.Enums;

namespace RMS.Domain.Entities;

/// <summary>کاربر سامانه و اطلاعات لازم برای ورود و تعیین سطح دسترسی.</summary>
public class UserEntities
{
    /// <summary>شناسهٔ یکتای کاربر.</summary>
    [Key]
    public Guid Id
    {
        get; set;
    }

    /// <summary>نام کاربر.</summary>
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>نام خانوادگی کاربر.</summary>
    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>شمارهٔ موبایل کاربر.</summary>
    [Required, MaxLength(20)]
    public string MobileNumber { get; set; } = string.Empty;

    /// <summary>هش رمز عبور؛ خود رمز خام اینجا نگه‌داری نمی‌شود.</summary>
    [Required, MaxLength(512)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>نقش کلی کاربر.</summary>
    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>مجوزهای جزئی کاربر در بخش‌های مختلف سامانه.</summary>
    public ICollection<UserPermissionEntities> Permissions { get; set; } = new List<UserPermissionEntities>();
}

/// <summary>عملیات مجاز یک کاربر روی یک بخش از سامانه.</summary>
public class UserPermissionEntities
{
    /// <summary>شناسهٔ کاربری که این مجوز برای او ثبت شده است.</summary>
    public Guid FkUser
    {
        get; set;
    }
    /// <summary>بخشی که مجوز روی آن اعمال می‌شود.</summary>
    public UserSection Section
    {
        get; set;
    }
    /// <summary>عملیات مجاز در بخش.</summary>
    public UserAccessOperation Operations
    {
        get; set;
    }
    /// <summary>کاربر صاحب مجوز.</summary>
    public UserEntities User { get; set; } = null!;
}