using NetTopologySuite.Geometries;
using RMS.Shared.Enums;
using RMS.Shared.Validation;
using System.ComponentModel.DataAnnotations;

namespace RMS.Shared.Contracts.DTOs;

/// <summary>داده‌های پایه ثبت یا ویرایش حادثه.</summary>
public class BaseAccident
{
    public Guid FkIdRoad { get; set; }

    [Range(-90, 90, ErrorMessage = "عرض جغرافیایی باید بین -90 تا 90 درجه باشد.")]
    public double Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "طول جغرافیایی باید بین -180 تا 180 درجه باشد.")]
    public double Longitude { get; set; }

    [Range(946684800, 4102444800, ErrorMessage = "زمان تصادف خارج از محدوده معتبر است.")]
    public long AccidentTimeUnix { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public WeatherConditionEnums Weather { get; set; } = WeatherConditionEnums.Unknown;
    public RoadConditionEnums RoadCondition { get; set; } = RoadConditionEnums.Unknown;
    public LightingConditionEnums LightingCondition { get; set; } = LightingConditionEnums.Unknown;
    public TrafficSignConditionEnums TrafficSignCondition { get; set; } = TrafficSignConditionEnums.Unknown;
    public AccidentCauseEnums AccidentCause { get; set; } = AccidentCauseEnums.Unknown;
    public AccidentTypeEnums AccidentType { get; set; } = AccidentTypeEnums.Unknown;
}

/// <summary>داده‌های قابل ویرایش خودرو.</summary>
public class BaseCarEdit
{
    [Required, MaxLength(50, ErrorMessage = "نهایت 50 کاراکتر.")]
    public string CarName { get; set; } = string.Empty;

    public CarClassEnums CarClass { get; set; } = CarClassEnums.Unknown;

    [Required, Range(1300, 2200, ErrorMessage = "بین 1300 تا 2200")]
    public int ProductionYear { get; set; }

    [Required, MaxLength(20, ErrorMessage = "نهایت 20 کاراکتر.")]
    public string PlateNumber { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "نهایت 50 کاراکتر.")]
    public string Color { get; set; } = string.Empty;

    [Required, MaxLength(20, ErrorMessage = "نهایت 20 کاراکتر.")]
    public string DriverPhone { get; set; } = string.Empty;

    [Required, MaxLength(20, ErrorMessage = "نهایت 20 کاراکتر.")]
    public string DriverLicNumber { get; set; } = string.Empty;

    [Range(typeof(DateOnly), "2020-01-01", "2100-12-31", ErrorMessage = "تاریخ نامعتبر است.")]
    public DateOnly DateLic { get; set; }

    [Range(0, 20, ErrorMessage = "بین 0 تا 20 معتبر است.")]
    public byte DateLicValidity { get; set; }

    [Range(1, 100, ErrorMessage = "بین 1 تا 100")]
    public byte DamagePercentage { get; set; } = 1;
}

public class BaseCar : BaseCarEdit
{
    public Guid FkAccident { get; set; }
}

/// <summary>اطلاعات مشترک اشخاص درگیر در حادثه.</summary>
public class SharedPassengerPeople
{
    [Required, MaxLength(20)]
    public string NationalCode { get; set; } = string.Empty;

    [Required, MaxLength(100, ErrorMessage = "نهایت 100 کاراکتر.")]
    public string PassengerFullName { get; set; } = string.Empty;

    public GenderEnums Gender { get; set; } = GenderEnums.Unknown;

    [Range(1, 150, ErrorMessage = "1 تا 150 معتبر است")]
    public byte Age { get; set; } = 1;

    [Range(0, 100, ErrorMessage = "بین 0 تا 100")]
    public byte InjuryPercentage { get; set; }

    [MaxLength(250)]
    public string DescriptionDamage { get; set; } = string.Empty;

    public DamageTypePersonEnum TypePersonDamage { get; set; } = DamageTypePersonEnum.None;
}

public class BasePassenger : SharedPassengerPeople
{
    public bool IsDriver { get; set; }
    public Guid FkCar { get; set; }
}

public class BasePeople : SharedPassengerPeople
{
    public Guid FkAccident { get; set; }
}

/// <summary>داده‌های یک راه یا محدوده جغرافیایی.</summary>
public class BaseRoads
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "تعیین مسیر خطی راه الزامی است.")]
    [ValidBoundaryGeometry]
    public Geometry Boundary { get; set; } = null!;

    [ValidCentroidPoint]
    public Point? Centroid { get; set; }
}