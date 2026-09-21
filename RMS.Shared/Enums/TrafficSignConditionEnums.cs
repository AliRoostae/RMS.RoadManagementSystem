namespace RMS.Shared.Enums;

/// <summary>
/// وضعیت تابلوها، خط‌کشی و علائم ترافیکی در محل وقوع حادثه را مشخص می‌کند.
/// </summary>
public enum TrafficSignConditionEnums : byte
{
    /// <summary>
    /// نامشخص
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// علائم استاندارد، سالم و کامل
    /// </summary>
    Standard = 1,

    /// <summary>
    /// فاقد علائم هشداردهنده لازم
    /// </summary>
    Missing = 2,

    /// <summary>
    /// علائم معیوب، فرسوده یا تخریب‌شده
    /// </summary>
    Damaged = 3,

    /// <summary>
    /// ناخوانا یا پوشانده‌شده با موانع دید
    /// </summary>
    Obscured = 4,

    /// <summary>
    /// علائم کارگاهی، انحراف مسیر یا موقت
    /// </summary>
    TemporaryWorkzone = 5
}
