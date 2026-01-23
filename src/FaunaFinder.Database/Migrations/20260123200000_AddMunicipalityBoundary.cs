using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace FaunaFinder.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMunicipalityBoundary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable PostGIS extension
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS postgis;");

            // Add boundary column
            migrationBuilder.AddColumn<Geometry>(
                name: "boundary",
                table: "municipalities",
                type: "geometry(Geometry, 4326)",
                nullable: true);

            // Create spatial index
            migrationBuilder.CreateIndex(
                name: "municipalities_boundary_gist_idx",
                table: "municipalities",
                column: "boundary")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "municipalities_boundary_gist_idx",
                table: "municipalities");

            migrationBuilder.DropColumn(
                name: "boundary",
                table: "municipalities");

            // Note: We don't drop the PostGIS extension as other tables might use it
        }
    }
}
