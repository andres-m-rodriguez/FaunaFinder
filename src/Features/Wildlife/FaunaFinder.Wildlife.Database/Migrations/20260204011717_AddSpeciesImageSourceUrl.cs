using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaunaFinder.Wildlife.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeciesImageSourceUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_source_url",
                table: "species",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_source_url",
                table: "species");
        }
    }
}
