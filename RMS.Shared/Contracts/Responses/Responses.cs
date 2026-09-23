using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;
using RMS.Shared.Enums;

namespace RMS.Shared.Contracts.Responses;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Skip,
    int Take);

public sealed record AccidentResponse
{
    public Guid Id
    {
        get; init;
    }
    public Guid FkIdRoad
    {
        get; init;
    }
    public double Latitude
    {
        get; init;
    }
    public double Longitude
    {
        get; init;
    }
    public long AccidentTimeUnix
    {
        get; init;
    }
    public WeatherConditionEnums Weather
    {
        get; init;
    }
    public RoadConditionEnums RoadCondition
    {
        get; init;
    }
    public LightingConditionEnums LightingCondition
    {
        get; init;
    }
    public TrafficSignConditionEnums TrafficSignCondition
    {
        get; init;
    }
    public AccidentCauseEnums AccidentCause
    {
        get; init;
    }
    public AccidentTypeEnums AccidentType
    {
        get; init;
    }
    public int PeopleCount
    {
        get; init;
    }
    public int DamagePercentageCar
    {
        get; init;
    }
    public int InjuryPercentage
    {
        get; init;
    }
}

public sealed record AccidentListItemResponse(
    Guid Id,
    long AccidentTimeUnix,
    Guid RoadId,
    string RoadName,
    AccidentTypeEnums AccidentType,
    WeatherConditionEnums Weather,
    int PeopleCount,
    int InjuryPercentage);

public sealed record AccidentMapPointResponse(
    Guid Id,
    Guid RoadId,
    long AccidentTimeUnix,
    double Latitude,
    double Longitude);

public sealed record CarResponse
{
    public Guid Id
    {
        get; set;
    }
    public Guid FkAccident
    {
        get; set;
    }
    public string CarName { get; set; } = string.Empty;
    public CarClassEnums CarClass { get; set; } = CarClassEnums.Unknown;
    public int ProductionYear
    {
        get; set;
    }
    public string PlateNumber { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string DriverPhone { get; set; } = string.Empty;
    public string DriverLicNumber { get; set; } = string.Empty;
    public DateOnly DateLic
    {
        get; set;
    }
    public byte DateLicValidity
    {
        get; set;
    }
    public byte DamagePercentage { get; set; } = 1;
    public int InjuryPercentage
    {
        get; init;
    }
}

public sealed record CarListItemResponse(
    Guid Id,
    Guid AccidentId,
    string AccidentCode,
    string PlateNumber,
    string CarName,
    CarClassEnums CarClass,
    int ProductionYear,
    int DamagePercentage);

public record SharedPassengerPeopleResponse
{
    public Guid Id
    {
        get; set;
    }
    public string NationalCode { get; set; } = string.Empty;
    public string PassengerFullName { get; set; } = string.Empty;
    public GenderEnums Gender { get; set; } = GenderEnums.Unknown;
    public byte Age { get; set; } = 1;
    public byte InjuryPercentage { get; set; } = 1;
    public string DescriptionDamage { get; set; } = string.Empty;
    public DamageTypePersonEnum TypePersonDamage { get; set; } = DamageTypePersonEnum.None;
}

public sealed record PassengerResponse : SharedPassengerPeopleResponse
{
    public Guid FkCar
    {
        get; set;
    }
    public bool IsDriver
    {
        get; set;
    }
}

public sealed record PeopleResponse : SharedPassengerPeopleResponse
{
    public Guid FkAccident
    {
        get; set;
    }
}

public sealed record PeopleListItemResponse(
    Guid Id,
    Guid AccidentId,
    string AccidentCode,
    string NationalCode,
    string FullName,
    GenderEnums Gender,
    byte Age,
    byte InjuryPercentage,
    DamageTypePersonEnum DamageType);

public sealed record HumanItemResponse(
    Guid Id,
    Guid AccidentId,
    string AccidentCode,
    string NationalCode,
    string FullName,
    GenderEnums Gender,
    byte Age,
    byte InjuryPercentage,
    DamageTypePersonEnum DamageType,
    bool IsPassenger,
    bool IsDriver);

public sealed record PassengerListItemResponse(
    Guid Id,
    Guid CarId,
    Guid AccidentId,
    string AccidentCode,
    string CarPlate,
    string NationalCode,
    string FullName,
    bool IsDriver,
    GenderEnums Gender,
    byte InjuryPercentage,
    DamageTypePersonEnum DamageType);

public sealed class ImageResponse
{
    public Guid Id
    {
        get; set;
    }
    public Guid FkAccident
    {
        get; set;
    }
    [Required]
    public string ImageUrls { get; set; } = string.Empty;
}

public sealed class RoadResponse
{
    public Guid Id
    {
        get; set;
    }
    public string Name { get; set; } = string.Empty;
    public Geometry Boundary { get; set; } = null!;
    public Point? Centroid
    {
        get; set;
    }
}

public sealed record RoadListItemResponse(
    Guid Id,
    string Name,
    int AccidentCount,
    bool HasCentroid);

public sealed record RoadMapItemResponse(
    Guid Id,
    string Name,
    Geometry Boundary,
    int AccidentCount);

public sealed record RoadLookupResponse(Guid Id, string Name);

public sealed record AccidentLookupResponse(
    Guid Id,
    long AccidentTimeUnix,
    Guid RoadId,
    string RoadName);

public sealed record CarLookupResponse(
    Guid Id,
    Guid AccidentId,
    string PlateNumber,
    string CarName);

public sealed record RoadGeometryResponse(
    Guid Id,
    Geometry Boundary,
    Point? Centroid);

public sealed record RoadLocationResponse(
    bool IsWithinTolerance,
    double DistanceMeters,
    double NearestLatitude,
    double NearestLongitude);

public sealed record CategoryCountResponse(byte Key, int Count);

public sealed record MonthlyAccidentMetricResponse(
    long PeriodStartUnix,
    int AccidentCount,
    int AverageInjury);

public sealed record RecentAccidentResponse(
    Guid Id,
    long AccidentTimeUnix,
    Guid RoadId,
    string RoadName,
    AccidentTypeEnums AccidentType,
    int InjuryPercentage);

public sealed record DashboardOverviewResponse(
    int AccidentCount,
    int RoadCount,
    int PeopleCount,
    int AverageInjury,
    IReadOnlyList<MonthlyAccidentMetricResponse> MonthlyTrend,
    IReadOnlyList<CategoryCountResponse> Causes,
    IReadOnlyList<CategoryCountResponse> RoadConditions,
    IReadOnlyList<RecentAccidentResponse> RecentAccidents);

public sealed record AccidentReportResponse(
    int AccidentCount,
    int PeopleCount,
    int CarCount,
    int AverageInjury,
    IReadOnlyList<MonthlyAccidentMetricResponse> MonthlyTrend,
    IReadOnlyList<CategoryCountResponse> Types,
    IReadOnlyList<CategoryCountResponse> Weather,
    IReadOnlyList<CategoryCountResponse> Causes);

public sealed record UserResponse
{
    public Guid Id
    {
        get; init;
    }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string MobileNumber { get; init; } = string.Empty;
    public UserRole Role
    {
        get; init;
    }
    public IReadOnlyCollection<UserPermissionResponse> Permissions { get; init; } = [];
}

public sealed record UserListItemResponse
{
    public Guid Id
    {
        get; init;
    }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string MobileNumber { get; init; } = string.Empty;
    public UserRole Role
    {
        get; init;
    }
    public int PermissionSectionCount
    {
        get; init;
    }
}

public sealed record UserPermissionResponse
{
    public UserSection Section
    {
        get; init;
    }
    public UserAccessOperation Operations
    {
        get; init;
    }
}

public sealed record AccessMetadataItem(byte Id, string Name);

public sealed record AccessMetadataResponse(
    IReadOnlyList<AccessMetadataItem> Roles,
    IReadOnlyList<AccessMetadataItem> Sections,
    IReadOnlyList<AccessMetadataItem> Operations);