namespace RMS.Shared.Enums;

/// <summary>
/// وضعیت روشنایی و دید محیطی در زمان وقوع حادثه را مشخص می‌کند.
/// </summary>
public enum LightingConditionEnums : byte
{
    /// <summary>
    /// نامشخص
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// روشنایی روز
    /// </summary>
    Daylight = 1,

    /// <summary>
    /// بین‌الطلوعین یا گرگ‌ومیش
    /// </summary>
    DawnDusk = 2,

    /// <summary>
    /// شب با روشنایی کافی معابر
    /// </summary>
    NightWithLighting = 3,

    /// <summary>
    /// شب بدون روشنایی یا تاریک
    /// </summary>
    NightWithoutLighting = 4,

    /// <summary>
    /// روشنایی تونل
    /// </summary>
    TunnelLighting = 5
}
