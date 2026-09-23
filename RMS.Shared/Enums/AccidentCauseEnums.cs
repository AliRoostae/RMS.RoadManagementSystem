namespace RMS.Shared.Enums;

/// <summary>
/// علت اصلی و کارشناسی وقوع حادثهٔ رانندگی را مشخص می‌کند.
/// </summary>
public enum AccidentCauseEnums : byte
{
    /// <summary>
    /// نامشخص
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// تخطی از سرعت مجاز یا مطمئنه
    /// </summary>
    Speeding = 1,

    /// <summary>
    /// عدم توجه کافی به جلو یا حواس‌پرتی
    /// </summary>
    Distraction = 2,

    /// <summary>
    /// خستگی و خواب‌آلودگی راننده
    /// </summary>
    FatigueAndDrowsiness = 3,

    /// <summary>
    /// سبقت غیرمجاز یا انحراف به چپ
    /// </summary>
    IllegalOvertaking = 4,

    /// <summary>
    /// عدم رعایت حق تقدم عبور
    /// </summary>
    FailureToYield = 5,

    /// <summary>
    /// عدم رعایت فاصله طولی مناسب
    /// </summary>
    Tailgating = 6,

    /// <summary>
    /// نقص فنی حادث یا مستمر در خودرو
    /// </summary>
    TechnicalDefect = 7,

    /// <summary>
    /// نقص، عیب یا دست‌انداز راه
    /// </summary>
    RoadDefect = 8,

    /// <summary>
    /// مصرف الکل، داروی خواب‌آور یا مواد روان‌گردان
    /// </summary>
    SubstanceImpairment = 9
}