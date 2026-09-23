namespace RMS.Shared.Enums;

/// <summary>
/// وضعیت سطح و بستر راه در محل وقوع حادثه را مشخص می‌کند.
/// </summary>
public enum RoadConditionEnums : byte
{
    /// <summary>
    /// نامشخص
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// خشک
    /// </summary>
    Dry = 1,

    /// <summary>
    /// خیس و بارانی
    /// </summary>
    Wet = 2,

    /// <summary>
    /// یخ‌زده
    /// </summary>
    Icy = 3,

    /// <summary>
    /// برفی
    /// </summary>
    Snowy = 4,

    /// <summary>
    /// گل‌آلود
    /// </summary>
    Muddy = 5,

    /// <summary>
    /// آغشته به روغن یا مواد لغزنده
    /// </summary>
    Oily = 6,

    /// <summary>
    /// دارای دست‌انداز یا در حال تعمیر
    /// </summary>
    UnderRepair = 7
}