namespace RMS.Shared.Enums;

/// <summary>
/// سطح شدت آسیب بدنی واردشده به شخص در حادثه را مشخص می‌کند.
/// </summary>
public enum DamageTypePersonEnum : byte
{
    /// <summary>آسیبی ثبت نشده است.</summary>
    None = 0,
    /// <summary>
    /// آسیب جزئی.
    /// </summary>
    Minor = 1,
    /// <summary>
    /// آسیب متوسط.
    /// </summary>
    Moderate = 2,
    /// <summary>
    /// آسیب شدید.
    /// </summary>
    Severe = 3,
    /// <summary>
    /// آسیب بحرانی.
    /// </summary>
    Critical = 4,
    /// <summary>
    /// فوت شخص.
    /// </summary>
    Fatal = 5
}