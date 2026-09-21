using RMS.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace RMS.Shared.Contracts.Queries;

/// <summary>پارامترهای مشترک صفحه‌بندی.</summary>
public class BaseQueries
{
    /// <summary>تعداد رکوردهای درخواستی؛ بین ۱ تا ۱۰۰۰ محدود می‌شود.</summary>
    public int Take { get; set => field = value < 1 ? 20 : value > 1000 ? 1000 : value; } = 20;
    /// <summary>تعداد رکوردهایی که قبل از شروع صفحه رد می‌شوند.</summary>
    public int Skip { get => field; set => field = value < 0 ? 0 : value; }
}

/// <summary>فیلتر و صفحه‌بندی فهرست حوادث.</summary>
public sealed class AccidentQuery : BaseQueries
{
    [Range(946684800, 4102444800, ErrorMessage = "زمان تصادف خارج از محدوده معتبر است.")]
    /// <summary>شروع بازهٔ زمانی به ثانیهٔ یونیکس.</summary>
    public long StartTime { get; set; }
    [Range(946684800, 4102444800, ErrorMessage = "زمان تصادف خارج از محدوده معتبر است.")]
    /// <summary>پایان بازهٔ زمانی به ثانیهٔ یونیکس.</summary>
    public long EndTime { get; set; }
    /// <summary>فیلتر بر اساس راه.</summary>
    public Guid? RoadId { get; set; }
    /// <summary>فیلتر بر اساس نوع حادثه.</summary>
    public AccidentTypeEnums? AccidentType { get; set; }
    /// <summary>فیلتر بر اساس وضعیت آب‌وهوا.</summary>
    public WeatherConditionEnums? Weather { get; set; }
    /// <summary>فیلتر بر اساس علت حادثه.</summary>
    public AccidentCauseEnums? Cause { get; set; }
    /// <summary>عبارت جست‌وجو در اطلاعات حادثه.</summary>
    public string? SearchTerm { get; set; }
    /// <summary>ترتیب نمایش حوادث.</summary>
    public AccidentSortOrder OrderBy { get; set; } = AccidentSortOrder.AccidentTimeDescending;
}

/// <summary>فیلتر نمایش حوادث روی نقشه.</summary>
public sealed class AccidentMapQuery
{
    [Range(946684800, 4102444800)]
    public long StartTime { get; set; }

    [Range(946684800, 4102444800)]
    public long EndTime { get; set; }

    public Guid? RoadId { get; set; }

    [Range(-90, 90)]
    public double? MinLatitude { get; set; }

    [Range(-90, 90)]
    public double? MaxLatitude { get; set; }

    [Range(-180, 180)]
    public double? MinLongitude { get; set; }

    [Range(-180, 180)]
    public double? MaxLongitude { get; set; }

    public int Take { get; set => field = value < 1 ? 500 : value > 5000 ? 5000 : value; } = 500;
}

public sealed class CarQueries : BaseQueries
{
    public Guid? AccidentId { get; set; }
    public string? Plate { get; set; }
    public string? SearchTerm { get; set; }
    public CarClassEnums? CarClass { get; set; }
}

public sealed class PeopleQueries : BaseQueries
{
    public Guid? AccidentId { get; set; }
    public string? NationalCode { get; set; }
    public string? SearchTerm { get; set; }
    public DamageTypePersonEnum? DamageType { get; set; }
}

public sealed class PassengerQueries : BaseQueries
{
    public Guid? CarId { get; set; }
    public Guid? AccidentId { get; set; }
    public string? NationalCode { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsDriver { get; set; }
    public DamageTypePersonEnum? DamageType { get; set; }
}
public sealed class RoadQueries : BaseQueries
{
    public string? SearchTerm { get; set; }
}

public sealed class RoadMapQuery
{
    public string? SearchTerm { get; set; }

    [Range(-90, 90)]
    public double? MinLatitude { get; set; }

    [Range(-90, 90)]
    public double? MaxLatitude { get; set; }

    [Range(-180, 180)]
    public double? MinLongitude { get; set; }

    [Range(-180, 180)]
    public double? MaxLongitude { get; set; }

    [Range(0, 22)]
    public int Zoom { get; set; } = 9;

    public int Take { get; set => field = value < 1 ? 200 : value > 500 ? 500 : value; } = 200;
}
public sealed class UserQuery : BaseQueries
{
    public string? SearchTerm { get; set; }
    public UserRole? Role { get; set; }
}

public class LookupQuery
{
    public string? SearchTerm { get; set; }
    public int Take { get; set => field = value < 1 ? 20 : value > 1000 ? 1000 : value; } = 20;
}

public sealed class AccidentLookupQuery : LookupQuery
{
    public Guid? RoadId { get; set; }
}

public sealed class CarLookupQuery : LookupQuery
{
    public Guid? AccidentId { get; set; }
}

public sealed class RoadLocationQuery
{
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Range(1, 5000)]
    public double MaxDistanceMeters { get; set; } = 100;
}

public sealed class DashboardQuery
{
    [Range(946684800, 4102444800)]
    public long StartTime { get; set; }

    [Range(946684800, 4102444800)]
    public long EndTime { get; set; }

    [Range(1, 24)]
    public int Months { get; set; } = 12;

    [Range(1, 20)]
    public int RecentCount { get; set; } = 4;
}

public sealed class AccidentReportQuery
{
    [Range(946684800, 4102444800)]
    public long StartTime { get; set; }

    [Range(946684800, 4102444800)]
    public long EndTime { get; set; }

    public Guid? RoadId { get; set; }
    public AccidentTypeEnums? AccidentType { get; set; }

    [Range(1, 24)]
    public int Months { get; set; } = 12;
}
