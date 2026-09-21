namespace RMS.Api.Authentication;

/// <summary>تنظیمات لازم برای ساخت و اعتبارسنجی توکن JWT.</summary>
public sealed class JwtOptions
{
    /// <summary>نام بخش تنظیمات در configuration.</summary>
    public const string SectionName = "Jwt";

    /// <summary>ناشر توکن.</summary>
    public string Issuer { get; set; } = string.Empty;
    /// <summary>مخاطب توکن.</summary>
    public string Audience { get; set; } = string.Empty;
    /// <summary>کلید امضای توکن.</summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>مدت اعتبار توکن بر حسب دقیقه.</summary>
    public int ExpirationMinutes { get; set; } = 480;
}
