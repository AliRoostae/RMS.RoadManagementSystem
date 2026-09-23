using System.Globalization;
using RMS.Shared.Enums;

namespace RMS.Client.Models;

/// <summary>ابزارهای تبدیل مقدارهای خام API به متن مناسب رابط کاربری.</summary>
public static class RmsUi
{
    /// <summary>شناسه را برای نمایش کوتاه می‌کند.</summary>
    public static string ShortId(Guid id) => id.ToString("N")[..8].ToUpperInvariant();

    /// <summary>زمان یونیکس را به تاریخ و ساعت فارسی قابل نمایش تبدیل می‌کند.</summary>
    public static string DateTime(long unixTime) => DateTimeOffset
        .FromUnixTimeSeconds(unixTime)
        .ToLocalTime()
        .ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture)
        .TranslateDigits();

    /// <summary>زمان یونیکس را به مقدار مناسب input تاریخ‌وساعت تبدیل می‌کند.</summary>
    public static string DateTimeInput(long unixTime) => DateTimeOffset
        .FromUnixTimeSeconds(unixTime)
        .ToLocalTime()
        .ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);

    public static string Weather(WeatherConditionEnums value) => value switch
    {
        WeatherConditionEnums.Clear => "صاف",
        WeatherConditionEnums.Cloudy => "ابری",
        WeatherConditionEnums.Rainy => "بارانی",
        WeatherConditionEnums.Drizzle => "نم‌نم باران",
        WeatherConditionEnums.Snowy => "برفی",
        WeatherConditionEnums.Thunderstorm => "طوفانی",
        WeatherConditionEnums.Foggy => "مه‌آلود",
        WeatherConditionEnums.Dusty => "گرد و غبار",
        WeatherConditionEnums.Windy => "باد شدید",
        _ => "نامشخص"
    };

    public static string RoadCondition(RoadConditionEnums value) => value switch
    {
        RoadConditionEnums.Dry => "خشک",
        RoadConditionEnums.Wet => "خیس",
        RoadConditionEnums.Icy => "یخ‌زده",
        RoadConditionEnums.Snowy => "برفی",
        RoadConditionEnums.Muddy => "گل‌آلود",
        RoadConditionEnums.Oily => "لغزنده/روغنی",
        RoadConditionEnums.UnderRepair => "در حال تعمیر",
        _ => "نامشخص"
    };

    public static string Lighting(LightingConditionEnums value) => value switch
    {
        LightingConditionEnums.Daylight => "روشنایی روز",
        LightingConditionEnums.DawnDusk => "گرگ‌ومیش",
        LightingConditionEnums.NightWithLighting => "شب با روشنایی",
        LightingConditionEnums.NightWithoutLighting => "شب بدون روشنایی",
        LightingConditionEnums.TunnelLighting => "روشنایی تونل",
        _ => "نامشخص"
    };

    public static string TrafficSign(TrafficSignConditionEnums value) => value switch
    {
        TrafficSignConditionEnums.Standard => "استاندارد",
        TrafficSignConditionEnums.Missing => "فاقد علائم",
        TrafficSignConditionEnums.Damaged => "آسیب‌دیده",
        TrafficSignConditionEnums.Obscured => "ناخوانا",
        TrafficSignConditionEnums.TemporaryWorkzone => "کارگاهی/موقت",
        _ => "نامشخص"
    };

    public static string AccidentCause(AccidentCauseEnums value) => value switch
    {
        AccidentCauseEnums.Speeding => "سرعت غیرمجاز",
        AccidentCauseEnums.Distraction => "عدم توجه به جلو",
        AccidentCauseEnums.FatigueAndDrowsiness => "خستگی و خواب‌آلودگی",
        AccidentCauseEnums.IllegalOvertaking => "سبقت غیرمجاز",
        AccidentCauseEnums.FailureToYield => "عدم رعایت حق تقدم",
        AccidentCauseEnums.Tailgating => "عدم رعایت فاصله",
        AccidentCauseEnums.TechnicalDefect => "نقص فنی خودرو",
        AccidentCauseEnums.RoadDefect => "نقص راه",
        AccidentCauseEnums.SubstanceImpairment => "مصرف مواد یا دارو",
        _ => "نامشخص"
    };

    public static string AccidentType(AccidentTypeEnums value) => value switch
    {
        AccidentTypeEnums.HeadOn => "برخورد روبه‌رو",
        AccidentTypeEnums.RearEnd => "برخورد از عقب",
        AccidentTypeEnums.AngleSide => "برخورد جانبی",
        AccidentTypeEnums.SideSwipe => "سایش جانبی",
        AccidentTypeEnums.ChainReaction => "زنجیره‌ای",
        AccidentTypeEnums.Rollover => "واژگونی",
        AccidentTypeEnums.FixedObjectHit => "برخورد با مانع ثابت",
        AccidentTypeEnums.PedestrianHit => "برخورد با عابر",
        AccidentTypeEnums.AnimalHit => "برخورد با حیوان",
        _ => "نامشخص"
    };

    public static string Gender(GenderEnums value) => value switch
    {
        GenderEnums.Male => "مرد",
        GenderEnums.Female => "زن",
        _ => "نامشخص"
    };

    public static string DamageType(DamageTypePersonEnum value) => value switch
    {
        DamageTypePersonEnum.None => "بدون آسیب",
        DamageTypePersonEnum.Minor => "جزئی",
        DamageTypePersonEnum.Moderate => "متوسط",
        DamageTypePersonEnum.Severe => "شدید",
        DamageTypePersonEnum.Critical => "بحرانی",
        DamageTypePersonEnum.Fatal => "فوت",
        _ => "نامشخص"
    };

    public static string CarClass(CarClassEnums value) => value switch
    {
        CarClassEnums.Sedan => "سدان",
        CarClassEnums.Hatchback => "هاچ‌بک",
        CarClassEnums.SsuV => "شاسی‌بلند",
        CarClassEnums.Crossover => "کراس‌اوور",
        CarClassEnums.Pickup => "وانت",
        CarClassEnums.Van => "ون",
        CarClassEnums.Truck => "کامیون",
        CarClassEnums.SemiTrailer => "کشنده",
        CarClassEnums.Bus => "اتوبوس",
        CarClassEnums.Minibus => "مینی‌بوس",
        CarClassEnums.Taxi => "تاکسی",
        CarClassEnums.Ambulance => "آمبولانس",
        CarClassEnums.Police => "پلیس",
        CarClassEnums.FireTruck => "آتش‌نشانی",
        CarClassEnums.Motorcycle => "موتورسیکلت",
        CarClassEnums.Tractor => "تراکتور",
        CarClassEnums.Unknown => "نامشخص",
        _ => value.ToString()
    };

    public static string InjuryTone(int percentage) => percentage switch
    {
        >= 70 => "danger",
        >= 30 => "warning",
        _ => "success"
    };
}

/// <summary>مقدارهای فرم‌های پویا را با اعتبارسنجی یکدست به نوع مناسب تبدیل می‌کند.</summary>
public static class FormValues
{
    public static string Required(IReadOnlyDictionary<string, string> values, string key, string label)
    {
        if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            throw new FormatException($"{label} الزامی است.");
        return value.Trim();
    }

    public static Guid Guid(IReadOnlyDictionary<string, string> values, string key, string label) =>
        System.Guid.TryParse(Required(values, key, label), out var value)
            ? value
            : throw new FormatException($"{label} معتبر نیست.");

    public static int Int(IReadOnlyDictionary<string, string> values, string key, string label) =>
        System.Int32.TryParse(Required(values, key, label), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new FormatException($"{label} باید عدد صحیح باشد.");

    public static byte Byte(IReadOnlyDictionary<string, string> values, string key, string label) =>
        System.Byte.TryParse(Required(values, key, label), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new FormatException($"{label} معتبر نیست.");

    public static double Double(IReadOnlyDictionary<string, string> values, string key, string label) =>
        System.Double.TryParse(Required(values, key, label), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new FormatException($"{label} باید عدد باشد.");

    public static TEnum Enum<TEnum>(IReadOnlyDictionary<string, string> values, string key, string label)
        where TEnum : struct, System.Enum
    {
        var raw = Byte(values, key, label);
        var value = (TEnum)System.Enum.ToObject(typeof(TEnum), raw);
        return System.Enum.IsDefined(value) ? value : throw new FormatException($"{label} معتبر نیست.");
    }

    public static DateOnly Date(IReadOnlyDictionary<string, string> values, string key, string label) =>
        DateOnly.TryParseExact(Required(values, key, label), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : throw new FormatException($"{label} معتبر نیست.");

    public static long UnixTime(IReadOnlyDictionary<string, string> values, string key, string label)
    {
        if (!System.DateTime.TryParse(Required(values, key, label), CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var local))
            throw new FormatException($"{label} معتبر نیست.");
        local = System.DateTime.SpecifyKind(local, DateTimeKind.Local);
        return new DateTimeOffset(local).ToUnixTimeSeconds();
    }

    public static string Optional(IReadOnlyDictionary<string, string> values, string key) =>
        values.GetValueOrDefault(key, string.Empty).Trim();
}