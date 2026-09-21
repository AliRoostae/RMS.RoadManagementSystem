namespace RMS.Shared.Enums;

/// <summary>
/// وضعیت شرایط آب‌وهوایی در زمان و محل وقوع تصادف.
/// </summary>
public enum WeatherConditionEnums : byte
{
    /// <summary>نامشخص / ثبت‌نشده</summary>
    Unknown = 0,

    /// <summary>صاف و آفتابی (Clear)</summary>
    Clear = 1,

    /// <summary>ابری / نیمه‌ابری (Clouds)</summary>
    Cloudy = 2,

    /// <summary>بارانی (Rain)</summary>
    Rainy = 3,

    /// <summary>نم‌نم باران / باران سبک (Drizzle)</summary>
    Drizzle = 4,

    /// <summary>برفی (Snow)</summary>
    Snowy = 5,

    /// <summary>طوفان و رعدوبرق (Thunderstorm)</summary>
    Thunderstorm = 6,

    /// <summary>مه‌آلود / غبار (Fog / Mist / Haze)</summary>
    Foggy = 7,

    /// <summary>گرد و غبار / طوفان شن (Dust / Sand)</summary>
    Dusty = 8,

    /// <summary>باد شدید / طوفان باد (Windy / Gale)</summary>
    Windy = 9
}
