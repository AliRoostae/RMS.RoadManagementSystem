using RMS.Shared.Contracts.Responses;
using System.ComponentModel.DataAnnotations;

namespace RMS.Shared.Contracts.Authentication;

/// <summary>اطلاعاتی که کاربر برای ورود به سامانه می‌فرستد.</summary>
public sealed class LoginRequest
{
    [Required(ErrorMessage = "شماره موبایل الزامی است.")]
    [MaxLength(20, ErrorMessage = "شماره موبایل حداکثر ۲۰ کاراکتر است.")]
    /// <summary>شماره موبایل کاربر.</summary>
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    [MaxLength(100, ErrorMessage = "رمز عبور حداکثر ۱۰۰ کاراکتر است.")]
    /// <summary>رمز عبور کاربر.</summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>نتیجهٔ موفق ورود، شامل توکن و اطلاعات کاربر.</summary>
public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    UserResponse User);