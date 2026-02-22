using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaunaFinder.Wildlife.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddConservationDataToSpecies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "el_code",
                table: "species",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "g_rank",
                table: "species",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "s_rank",
                table: "species",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "el_code",
                table: "species");

            migrationBuilder.DropColumn(
                name: "g_rank",
                table: "species");

            migrationBuilder.DropColumn(
                name: "s_rank",
                table: "species");
        }
    }
}
