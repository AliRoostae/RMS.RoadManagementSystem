using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StoreRoadNetworkAsMultiLineString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Road_Boundary_Type",
                table: "RoadsDs");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Road_Boundary_Type",
                table: "RoadsDs",
                sql: "[Boundary].STGeometryType() IN ('LineString', 'MultiLineString', 'Polygon', 'MultiPolygon')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Road_Boundary_Type",
                table: "RoadsDs");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Road_Boundary_Type",
                table: "RoadsDs",
                sql: "[Boundary].STGeometryType() IN ('Polygon', 'MultiPolygon')");
        }
    }
}
