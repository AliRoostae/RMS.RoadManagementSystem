using Microsoft.EntityFrameworkCore;
using RMS.Domain.Entities;
using RMS.Shared.Enums;

namespace RMS.Infrastructure.Persistence.EF.Core;

/// <summary>
/// نشست پایگاه دادهٔ سامانهٔ مدیریت راه را برای نگاشت و دسترسی به موجودیت‌های EF Core فراهم می‌کند.
/// </summary>
public sealed class RmsDbContext : DbContext
{
    /// <summary>نمونه‌ای از نشست پایگاه داده را با گزینه‌های پیکربندی‌شده ایجاد می‌کند.</summary>
    /// <param name="options">گزینه‌های پیکربندی EF Core برای این نشست.</param>
    public RmsDbContext(DbContextOptions<RmsDbContext> options) : base(options)
    {

    }

    /// <summary>مجموعهٔ حوادث ثبت‌شده را ارائه می‌کند.</summary>
    public DbSet<AccidentEntities> AccidentDs => Set<AccidentEntities>();

    /// <summary>مجموعهٔ عابر را ارائه می‌کند.</summary>
    public DbSet<PeopleEntities> PeopleDs => Set<PeopleEntities>();

    /// <summary>مجموعهٔ خودروهای درگیر در حوادث را ارائه می‌کند.</summary>
    public DbSet<CarEntities> CarDs => Set<CarEntities>();

    /// <summary>مجموعهٔ رانندگان و سرنشینان خودروها را ارائه می‌کند.</summary>
    public DbSet<PassengerEntities> PassengerDs => Set<PassengerEntities>();

    /// <summary>مجموعهٔ راه‌ها و محدوده‌های جغرافیایی را ارائه می‌کند.</summary>
    public DbSet<RoadsEntities> RoadsDs => Set<RoadsEntities>();

    public DbSet<ImageEntities> ImageDs => Set<ImageEntities>();

    /// <summary>مجموعهٔ کاربران سامانه را ارائه می‌کند.</summary>
    public DbSet<UserEntities> UserDs => Set<UserEntities>();

    /// <summary>مجموعهٔ دسترسی‌های بخش‌بندی‌شدهٔ کاربران را ارائه می‌کند.</summary>
    public DbSet<UserPermissionEntities> UserPermissionDs => Set<UserPermissionEntities>();


    private static string GetEnumValues<TEnum>()
    where TEnum : struct, Enum
    {
        return string.Join(
            ", ",
            Enum.GetValues<TEnum>()
                .Select(static value => Convert.ToInt32(value)));
    }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureRoads(modelBuilder);
        ConfigureAccident(modelBuilder);
        ConfigureCar(modelBuilder);
        ConfigurePeople(modelBuilder);
        ConfigurePassenger(modelBuilder);
        ConfigureImage(modelBuilder);
        ConfigureUser(modelBuilder);
    }

    private void ConfigureImage(ModelBuilder m)
    {
        m.Entity<ImageEntities>(entity =>
        {
            entity.HasKey(a => a.Id);
        });
    }

    private static void ConfigureRoads(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoadsEntities>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Boundary)
                .HasColumnType("geography");

            entity.Property(x => x.Centroid)
                .HasColumnType("geography");

            entity.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("UX_Road_Name");

            entity.HasMany(x => x.AccountList)
                .WithOne(x => x.Road)
                .HasForeignKey(x => x.FkIdRoad)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Road_Name_NotBlank",
                    "LEN(LTRIM(RTRIM([Name]))) > 0");

                table.HasCheckConstraint(
                    "CK_Road_Boundary_Srid",
                    "[Boundary].STSrid = 4326");

                table.HasCheckConstraint(
                    "CK_Road_Boundary_NotEmpty",
                    "[Boundary].STIsEmpty() = 0");

                table.HasCheckConstraint(
                    "CK_Road_Boundary_Valid",
                    "[Boundary].STIsValid() = 1");

                table.HasCheckConstraint(
                    "CK_Road_Boundary_Type",
                    "[Boundary].STGeometryType() IN ('LineString', 'MultiLineString', 'Polygon', 'MultiPolygon')");

                table.HasCheckConstraint(
                    "CK_Road_Centroid",
                    "[Centroid] IS NULL OR (" +
                    "[Centroid].STSrid = 4326 AND " +
                    "[Centroid].STIsEmpty() = 0 AND " +
                    "[Centroid].STGeometryType() = 'Point')");

                // نقطه نماینده باید روی یکی از خطوط مسیر باشد.
                table.HasCheckConstraint(
                    "CK_Road_Centroid_InsideBoundary",
                    "[Centroid] IS NULL OR " +
                    "[Boundary].STIntersects([Centroid]) = 1");
            });
        });
    }

    private static void ConfigureAccident(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccidentEntities>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.AccidentTimeUnix)
                .HasDatabaseName("IX_Accident_AccidentTimeUnix");

            entity.HasMany(x => x.CarList)
                .WithOne(x => x.Accident)
                .HasForeignKey(x => x.FkAccident)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.PeopleList)
                .WithOne(x => x.Accident)
                .HasForeignKey(x => x.FkAccident)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.ImageList)
                .WithOne(x => x.Accident)
                .HasForeignKey(x => x.FkAccident);

            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Accident_Latitude",
                    "[Latitude] BETWEEN -90 AND 90");

                table.HasCheckConstraint(
                    "CK_Accident_Longitude",
                    "[Longitude] BETWEEN -180 AND 180");

                table.HasCheckConstraint(
                    "CK_Accident_Time",
                    "[AccidentTimeUnix] BETWEEN " +
                    "CAST(946684800 AS bigint) AND CAST(4102444800 AS bigint)");

                table.HasCheckConstraint(
                    "CK_Accident_Weather",
                    $"[Weather] IN ({GetEnumValues<WeatherConditionEnums>()})");

                table.HasCheckConstraint(
                    "CK_Accident_RoadCondition",
                    $"[RoadCondition] IN ({GetEnumValues<RoadConditionEnums>()})");

                table.HasCheckConstraint(
                    "CK_Accident_LightingCondition",
                    $"[LightingCondition] IN ({GetEnumValues<LightingConditionEnums>()})");

                table.HasCheckConstraint(
                    "CK_Accident_TrafficSignCondition",
                    $"[TrafficSignCondition] IN ({GetEnumValues<TrafficSignConditionEnums>()})");

                table.HasCheckConstraint(
                    "CK_Accident_Cause",
                    $"[AccidentCause] IN ({GetEnumValues<AccidentCauseEnums>()})");

                table.HasCheckConstraint(
                    "CK_Accident_Type",
                    $"[AccidentType] IN ({GetEnumValues<AccidentTypeEnums>()})");

                table.HasCheckConstraint(
                    "CK_Accident_RoadId_NotEmpty",
                    "[FkIdRoad] <> '00000000-0000-0000-0000-000000000000'");
            });
        });
    }

    private static void ConfigureCar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CarEntities>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new
            {
                x.PlateNumber,
                x.FkAccident
            })
            .IsUnique()
            .HasDatabaseName("UX_Car_PlateNumber_AccidentId");

            entity.HasMany(x => x.PassengerList)
                .WithOne(x => x.Car)
                .HasForeignKey(x => x.FkCar)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Car_ProductionYear",
                    "[ProductionYear] BETWEEN 1300 AND 2200");

                table.HasCheckConstraint(
                    "CK_Car_LicenseDate",
                    "[DateLic] BETWEEN '20200101' AND '21001231'");

                table.HasCheckConstraint(
                    "CK_Car_LicenseValidity",
                    "[DateLicValidity] BETWEEN 0 AND 20");

                table.HasCheckConstraint(
                    "CK_Car_DamagePercentage",
                    "[DamagePercentage] BETWEEN 1 AND 100");

                table.HasCheckConstraint(
                    "CK_Car_Class",
                    $"[CarClass] IN ({GetEnumValues<CarClassEnums>()})");

                table.HasCheckConstraint(
                    "CK_Car_AccidentId_NotEmpty",
                    "[FkAccident] <> '00000000-0000-0000-0000-000000000000'");

                table.HasCheckConstraint(
                    "CK_Car_PlateNumber_NotBlank",
                    "LEN(LTRIM(RTRIM([PlateNumber]))) > 0");

                table.HasCheckConstraint(
                    "CK_Car_Name_NotBlank",
                    "LEN(LTRIM(RTRIM([CarName]))) > 0");
            });
        });
    }


    private static void ConfigurePeople(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PeopleEntities>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new
            {
                x.NationalCode,
                x.FkAccident
            })
            .IsUnique()
            .HasDatabaseName("UX_People_NationalCode_AccidentId");

            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_People_Age",
                    "[Age] BETWEEN 1 AND 150");

                table.HasCheckConstraint(
                    "CK_People_InjuryPercentage",
                    "[InjuryPercentage] BETWEEN 0 AND 100");

                table.HasCheckConstraint(
                    "CK_People_Gender",
                    $"[Gender] IN ({GetEnumValues<GenderEnums>()})");

                table.HasCheckConstraint(
                    "CK_People_DamageType",
                    $"[TypePersonDamage] IN ({GetEnumValues<DamageTypePersonEnum>()})");

                table.HasCheckConstraint(
                    "CK_People_FatalInjury",
                    "[TypePersonDamage] <> 5 OR [InjuryPercentage] = 100");

                table.HasCheckConstraint(
                    "CK_People_AccidentId_NotEmpty",
                    "[FkAccident] <> '00000000-0000-0000-0000-000000000000'");

                table.HasCheckConstraint(
                    "CK_People_NationalCode_NotBlank",
                    "LEN(LTRIM(RTRIM([NationalCode]))) > 0");

                table.HasCheckConstraint(
                    "CK_People_FullName_NotBlank",
                    "LEN(LTRIM(RTRIM([PassengerFullName]))) > 0");
            });
        });
    }



    private static void ConfigurePassenger(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PassengerEntities>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new
            {
                x.NationalCode,
                x.FkCar
            })
            .IsUnique()
            .HasDatabaseName("UX_Passenger_NationalCode_CarId");

            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_Passenger_Age",
                    "[Age] BETWEEN 1 AND 150");

                table.HasCheckConstraint(
                    "CK_Passenger_InjuryPercentage",
                    "[InjuryPercentage] BETWEEN 0 AND 100");

                table.HasCheckConstraint(
                    "CK_Passenger_Gender",
                    $"[Gender] IN ({GetEnumValues<GenderEnums>()})");

                table.HasCheckConstraint(
                    "CK_Passenger_DamageType",
                    $"[TypePersonDamage] IN ({GetEnumValues<DamageTypePersonEnum>()})");

                table.HasCheckConstraint(
                    "CK_Passenger_FatalInjury",
                    "[TypePersonDamage] <> 5 OR [InjuryPercentage] = 100");

                table.HasCheckConstraint(
                    "CK_Passenger_CarId_NotEmpty",
                    "[FkCar] <> '00000000-0000-0000-0000-000000000000'");

                table.HasCheckConstraint(
                    "CK_Passenger_NationalCode_NotBlank",
                    "LEN(LTRIM(RTRIM([NationalCode]))) > 0");
            });

            // فقط یک Passenger با IsDriver=true برای هر خودرو
            entity.HasIndex(
                    x => x.FkCar,
                    "UX_Passenger_OneDriverPerCar")
                .IsUnique()
                .HasFilter("[IsDriver] = 1");
        });
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntities>(entity =>
        {
            entity.HasKey(user => user.Id);

            entity.Property(user => user.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(user => user.LastName).HasMaxLength(50).IsRequired();
            entity.Property(user => user.MobileNumber).HasMaxLength(20).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();

            entity.HasIndex(user => user.MobileNumber)
                .IsUnique()
                .HasDatabaseName("UX_User_MobileNumber");

            entity.HasMany(user => user.Permissions)
                .WithOne(permission => permission.User)
                .HasForeignKey(permission => permission.FkUser)
                .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_User_FirstName_NotBlank",
                    "LEN(LTRIM(RTRIM([FirstName]))) > 0");

                table.HasCheckConstraint(
                    "CK_User_LastName_NotBlank",
                    "LEN(LTRIM(RTRIM([LastName]))) > 0");

                table.HasCheckConstraint(
                    "CK_User_MobileNumber",
                    "[MobileNumber] LIKE '09%' AND LEN([MobileNumber]) = 11");

                table.HasCheckConstraint(
                    "CK_User_PasswordHash_NotBlank",
                    "LEN(LTRIM(RTRIM([PasswordHash]))) > 0");

                table.HasCheckConstraint(
                    "CK_User_Role",
                    $"[Role] IN ({GetEnumValues<UserRole>()})");
            });
        });

        modelBuilder.Entity<UserPermissionEntities>(entity =>
        {
            entity.HasKey(permission => new { permission.FkUser, permission.Section });

            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_UserPermission_Section",
                    $"[Section] IN ({GetEnumValues<UserSection>()})");

                table.HasCheckConstraint(
                    "CK_UserPermission_Operations",
                    "[Operations] BETWEEN 1 AND 31");
            });
        });
    }



}
