using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoadsDs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Boundary = table.Column<Geometry>(type: "geography", nullable: false),
                    Centroid = table.Column<Point>(type: "geography", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadsDs", x => x.Id);
                    table.CheckConstraint("CK_Road_Boundary_NotEmpty", "[Boundary].STIsEmpty() = 0");
                    table.CheckConstraint("CK_Road_Boundary_Srid", "[Boundary].STSrid = 4326");
                    table.CheckConstraint("CK_Road_Boundary_Type", "[Boundary].STGeometryType() IN ('Polygon', 'MultiPolygon')");
                    table.CheckConstraint("CK_Road_Boundary_Valid", "[Boundary].STIsValid() = 1");
                    table.CheckConstraint("CK_Road_Centroid", "[Centroid] IS NULL OR ([Centroid].STSrid = 4326 AND [Centroid].STIsEmpty() = 0 AND [Centroid].STGeometryType() = 'Point')");
                    table.CheckConstraint("CK_Road_Centroid_InsideBoundary", "[Centroid] IS NULL OR [Boundary].STIntersects([Centroid]) = 1");
                    table.CheckConstraint("CK_Road_Name_NotBlank", "LEN(LTRIM(RTRIM([Name]))) > 0");
                });

            migrationBuilder.CreateTable(
                name: "UserDs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Role = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDs", x => x.Id);
                    table.CheckConstraint("CK_User_FirstName_NotBlank", "LEN(LTRIM(RTRIM([FirstName]))) > 0");
                    table.CheckConstraint("CK_User_LastName_NotBlank", "LEN(LTRIM(RTRIM([LastName]))) > 0");
                    table.CheckConstraint("CK_User_MobileNumber", "[MobileNumber] LIKE '09%' AND LEN([MobileNumber]) = 11");
                    table.CheckConstraint("CK_User_PasswordHash_NotBlank", "LEN(LTRIM(RTRIM([PasswordHash]))) > 0");
                    table.CheckConstraint("CK_User_Role", "[Role] IN (1, 2, 3)");
                });

            migrationBuilder.CreateTable(
                name: "AccidentDs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkIdRoad = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    AccidentTimeUnix = table.Column<long>(type: "bigint", nullable: false),
                    Weather = table.Column<byte>(type: "tinyint", nullable: false),
                    RoadCondition = table.Column<byte>(type: "tinyint", nullable: false),
                    LightingCondition = table.Column<byte>(type: "tinyint", nullable: false),
                    TrafficSignCondition = table.Column<byte>(type: "tinyint", nullable: false),
                    AccidentCause = table.Column<byte>(type: "tinyint", nullable: false),
                    AccidentType = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccidentDs", x => x.Id);
                    table.CheckConstraint("CK_Accident_Cause", "[AccidentCause] IN (0, 1, 2, 3, 4, 5, 6, 7, 8, 9)");
                    table.CheckConstraint("CK_Accident_Latitude", "[Latitude] BETWEEN -90 AND 90");
                    table.CheckConstraint("CK_Accident_LightingCondition", "[LightingCondition] IN (0, 1, 2, 3, 4, 5)");
                    table.CheckConstraint("CK_Accident_Longitude", "[Longitude] BETWEEN -180 AND 180");
                    table.CheckConstraint("CK_Accident_RoadCondition", "[RoadCondition] IN (0, 1, 2, 3, 4, 5, 6, 7)");
                    table.CheckConstraint("CK_Accident_RoadId_NotEmpty", "[FkIdRoad] <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Accident_Time", "[AccidentTimeUnix] BETWEEN CAST(946684800 AS bigint) AND CAST(4102444800 AS bigint)");
                    table.CheckConstraint("CK_Accident_TrafficSignCondition", "[TrafficSignCondition] IN (0, 1, 2, 3, 4, 5)");
                    table.CheckConstraint("CK_Accident_Type", "[AccidentType] IN (0, 1, 2, 3, 4, 5, 6, 7, 8, 9)");
                    table.CheckConstraint("CK_Accident_Weather", "[Weather] IN (0, 1, 2, 3, 4, 5, 6, 7, 8, 9)");
                    table.ForeignKey(
                        name: "FK_AccidentDs_RoadsDs_FkIdRoad",
                        column: x => x.FkIdRoad,
                        principalTable: "RoadsDs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissionDs",
                columns: table => new
                {
                    FkUser = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Section = table.Column<byte>(type: "tinyint", nullable: false),
                    Operations = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissionDs", x => new { x.FkUser, x.Section });
                    table.CheckConstraint("CK_UserPermission_Operations", "[Operations] BETWEEN 1 AND 31");
                    table.CheckConstraint("CK_UserPermission_Section", "[Section] IN (1, 2, 3, 4, 5, 6, 7, 8)");
                    table.ForeignKey(
                        name: "FK_UserPermissionDs_UserDs_FkUser",
                        column: x => x.FkUser,
                        principalTable: "UserDs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarDs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkAccident = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CarName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CarClass = table.Column<byte>(type: "tinyint", nullable: false),
                    ProductionYear = table.Column<int>(type: "int", nullable: false),
                    PlateNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DriverPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DriverLicNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateLic = table.Column<DateOnly>(type: "date", nullable: false),
                    DateLicValidity = table.Column<byte>(type: "tinyint", nullable: false),
                    DamagePercentage = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarDs", x => x.Id);
                    table.CheckConstraint("CK_Car_AccidentId_NotEmpty", "[FkAccident] <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Car_Class", "[CarClass] IN (0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39)");
                    table.CheckConstraint("CK_Car_DamagePercentage", "[DamagePercentage] BETWEEN 1 AND 100");
                    table.CheckConstraint("CK_Car_LicenseDate", "[DateLic] BETWEEN '20200101' AND '21001231'");
                    table.CheckConstraint("CK_Car_LicenseValidity", "[DateLicValidity] BETWEEN 0 AND 20");
                    table.CheckConstraint("CK_Car_Name_NotBlank", "LEN(LTRIM(RTRIM([CarName]))) > 0");
                    table.CheckConstraint("CK_Car_PlateNumber_NotBlank", "LEN(LTRIM(RTRIM([PlateNumber]))) > 0");
                    table.CheckConstraint("CK_Car_ProductionYear", "[ProductionYear] BETWEEN 1300 AND 2200");
                    table.ForeignKey(
                        name: "FK_CarDs_AccidentDs_FkAccident",
                        column: x => x.FkAccident,
                        principalTable: "AccidentDs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImageDs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkAccident = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrls = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageDs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageDs_AccidentDs_FkAccident",
                        column: x => x.FkAccident,
                        principalTable: "AccidentDs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeopleDs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkAccident = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NationalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PassengerFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<byte>(type: "tinyint", nullable: false),
                    Age = table.Column<byte>(type: "tinyint", nullable: false),
                    InjuryPercentage = table.Column<byte>(type: "tinyint", nullable: false),
                    DescriptionDamage = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TypePersonDamage = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeopleDs", x => x.Id);
                    table.CheckConstraint("CK_People_AccidentId_NotEmpty", "[FkAccident] <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_People_Age", "[Age] BETWEEN 1 AND 150");
                    table.CheckConstraint("CK_People_DamageType", "[TypePersonDamage] IN (0, 1, 2, 3, 4, 5)");
                    table.CheckConstraint("CK_People_FatalInjury", "[TypePersonDamage] <> 5 OR [InjuryPercentage] = 100");
                    table.CheckConstraint("CK_People_FullName_NotBlank", "LEN(LTRIM(RTRIM([PassengerFullName]))) > 0");
                    table.CheckConstraint("CK_People_Gender", "[Gender] IN (0, 1, 2)");
                    table.CheckConstraint("CK_People_InjuryPercentage", "[InjuryPercentage] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_People_NationalCode_NotBlank", "LEN(LTRIM(RTRIM([NationalCode]))) > 0");
                    table.ForeignKey(
                        name: "FK_PeopleDs_AccidentDs_FkAccident",
                        column: x => x.FkAccident,
                        principalTable: "AccidentDs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PassengerDs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkCar = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NationalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PassengerFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<byte>(type: "tinyint", nullable: false),
                    Age = table.Column<byte>(type: "tinyint", nullable: false),
                    IsDriver = table.Column<bool>(type: "bit", nullable: false),
                    InjuryPercentage = table.Column<byte>(type: "tinyint", nullable: false),
                    DescriptionDamage = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TypePersonDamage = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PassengerDs", x => x.Id);
                    table.CheckConstraint("CK_Passenger_Age", "[Age] BETWEEN 1 AND 150");
                    table.CheckConstraint("CK_Passenger_CarId_NotEmpty", "[FkCar] <> '00000000-0000-0000-0000-000000000000'");
                    table.CheckConstraint("CK_Passenger_DamageType", "[TypePersonDamage] IN (0, 1, 2, 3, 4, 5)");
                    table.CheckConstraint("CK_Passenger_FatalInjury", "[TypePersonDamage] <> 5 OR [InjuryPercentage] = 100");
                    table.CheckConstraint("CK_Passenger_Gender", "[Gender] IN (0, 1, 2)");
                    table.CheckConstraint("CK_Passenger_InjuryPercentage", "[InjuryPercentage] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_Passenger_NationalCode_NotBlank", "LEN(LTRIM(RTRIM([NationalCode]))) > 0");
                    table.ForeignKey(
                        name: "FK_PassengerDs_CarDs_FkCar",
                        column: x => x.FkCar,
                        principalTable: "CarDs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accident_AccidentTimeUnix",
                table: "AccidentDs",
                column: "AccidentTimeUnix");

            migrationBuilder.CreateIndex(
                name: "IX_AccidentDs_FkIdRoad",
                table: "AccidentDs",
                column: "FkIdRoad");

            migrationBuilder.CreateIndex(
                name: "IX_CarDs_FkAccident",
                table: "CarDs",
                column: "FkAccident");

            migrationBuilder.CreateIndex(
                name: "UX_Car_PlateNumber_AccidentId",
                table: "CarDs",
                columns: new[] { "PlateNumber", "FkAccident" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageDs_FkAccident",
                table: "ImageDs",
                column: "FkAccident");

            migrationBuilder.CreateIndex(
                name: "UX_Passenger_NationalCode_CarId",
                table: "PassengerDs",
                columns: new[] { "NationalCode", "FkCar" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Passenger_OneDriverPerCar",
                table: "PassengerDs",
                column: "FkCar",
                unique: true,
                filter: "[IsDriver] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PeopleDs_FkAccident",
                table: "PeopleDs",
                column: "FkAccident");

            migrationBuilder.CreateIndex(
                name: "UX_People_NationalCode_AccidentId",
                table: "PeopleDs",
                columns: new[] { "NationalCode", "FkAccident" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Road_Name",
                table: "RoadsDs",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_User_MobileNumber",
                table: "UserDs",
                column: "MobileNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageDs");

            migrationBuilder.DropTable(
                name: "PassengerDs");

            migrationBuilder.DropTable(
                name: "PeopleDs");

            migrationBuilder.DropTable(
                name: "UserPermissionDs");

            migrationBuilder.DropTable(
                name: "CarDs");

            migrationBuilder.DropTable(
                name: "UserDs");

            migrationBuilder.DropTable(
                name: "AccidentDs");

            migrationBuilder.DropTable(
                name: "RoadsDs");
        }
    }
}