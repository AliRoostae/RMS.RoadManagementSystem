namespace RMS.Shared.Enums;

/// <summary>
/// الگوی برخورد و نوع حادثهٔ رانندگی را مشخص می‌کند.
/// </summary>
public enum AccidentTypeEnums : byte
{
    /// <summary>
    /// نامشخص
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// برخورد شاخ‌به‌شاخ یا روبرو
    /// </summary>
    HeadOn = 1,

    /// <summary>
    /// برخورد از عقب (پشت سر)
    /// </summary>
    RearEnd = 2,

    /// <summary>
    /// برخورد زاویه‌ای یا از پهلو
    /// </summary>
    AngleSide = 3,

    /// <summary>
    /// برخورد مماسی یا سایش پهلو به پهلو
    /// </summary>
    SideSwipe = 4,

    /// <summary>
    /// تصادف زنجیره‌ای چند خودرویی
    /// </summary>
    ChainReaction = 5,

    /// <summary>
    /// واژگونی یا غلتیدن خودرو
    /// </summary>
    Rollover = 6,

    /// <summary>
    /// برخورد با مانع ثابت، گاردریل یا تیر چراغ برق
    /// </summary>
    FixedObjectHit = 7,

    /// <summary>
    /// برخورد با عابر پیاده
    /// </summary>
    PedestrianHit = 8,

    /// <summary>
    /// برخورد با احشام یا حیوانات
    /// </summary>
    AnimalHit = 9
}