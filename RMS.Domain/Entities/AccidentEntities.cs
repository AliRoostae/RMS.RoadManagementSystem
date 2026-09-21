using RMS.Shared.Enums;
using System.ComponentModel.DataAnnotations;


namespace RMS.Domain.Entities;

/// <summary>
/// موجودیت پایدار حادثهٔ رانندگی و ارتباط آن با راه، خودروها و عابر را نمایش می‌دهد.
/// </summary>
public class AccidentEntities
{
    /// <summary>شناسهٔ یکتای حادثه.</summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// شناسهٔ راه یا محدودهٔ جغرافیایی مرتبط با حادثه.
    /// </summary>

    public Guid FkIdRoad { get; set; }
    /// <summary>
    /// عرض جغرافیایی (Latitude) محل تصادف
    /// </summary>
    [Range(-90, 90, ErrorMessage = "عرض جغرافیایی باید بین -90 تا 90 درجه باشد.")]
    public double Latitude { get; set; }

    /// <summary>
    /// طول جغرافیایی (Longitude) محل تصادف
    /// </summary>
    [Range(-180, 180, ErrorMessage = "طول جغرافیایی باید بین -180 تا 180 درجه باشد.")]
    public double Longitude { get; set; }


    /// <summary>
    /// زمان وقوع حادثه را برحسب ثانیهٔ یونیکس نگهداری می‌کند.
    /// </summary>
    [Range(946684800, 4102444800, ErrorMessage = "زمان تصادف خارج از محدوده معتبر است.")]
    public long AccidentTimeUnix { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();


    /// <summary>
    /// وضعیت جوی و آب‌وهوای محل تصادف
    /// </summary>
    public WeatherConditionEnums Weather { get; set; } = WeatherConditionEnums.Unknown;

    /// <summary>
    /// وضعیت سطح و بستر جاده در محل وقوع تصادف
    /// </summary>
    public RoadConditionEnums RoadCondition { get; set; } = RoadConditionEnums.Unknown;

    /// <summary>
    /// وضعیت روشنایی و دید محیطی در زمان وقوع تصادف
    /// </summary>
    public LightingConditionEnums LightingCondition { get; set; } = LightingConditionEnums.Unknown;

    /// <summary>
    /// وضعیت تابلوها، خط‌کشی و علائم ترافیکی در محل تصادف
    /// </summary>
    public TrafficSignConditionEnums TrafficSignCondition { get; set; } = TrafficSignConditionEnums.Unknown;

    /// <summary>
    /// علت اصلی و کارشناسی وقوع تصادف
    /// </summary>
    public AccidentCauseEnums AccidentCause { get; set; } = AccidentCauseEnums.Unknown;

    /// <summary>
    /// الگوی هندسی و نوع برخورد وسایل نقلیه در تصادف
    /// </summary>
    public AccidentTypeEnums AccidentType { get; set; } = AccidentTypeEnums.Unknown;


    /// <summary>راه یا محدودهٔ جغرافیایی مرتبط با حادثه.</summary>
    public RoadsEntities Road { get; set; } = null!;

    /// <summary>
    /// مجموعهٔ خودروهای درگیر در حادثه.
    /// </summary>
    public ICollection<CarEntities> CarList { get; set; } = new List<CarEntities>();

    /// <summary>
    /// مجموعهٔ افراد حاضر در حادثه که داخل خودرو نبوده‌اند؛ مانند عابران پیاده.
    /// </summary>
    public ICollection<PeopleEntities> PeopleList { get; set; } = new List<PeopleEntities>();

    /// <summary>
    /// مجموعهٔ تصاویر حادثه .
    /// </summary>
    public ICollection<ImageEntities> ImageList { get; set; } = new List<ImageEntities>();


}
