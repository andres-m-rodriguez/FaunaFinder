using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FaunaFinder.Wildlife.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeciesCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create species_categories table
            migrationBuilder.CreateTable(
                name: "species_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_species_categories", x => x.id);
                });

            // Create species_category_links table
            migrationBuilder.CreateTable(
                name: "species_category_links",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    species_id = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_species_category_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_species_category_links_species_species_id",
                        column: x => x.species_id,
                        principalTable: "species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_species_category_links_species_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "species_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "species_categories_code_uidx",
                table: "species_categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_species_category_links_category_id",
                table: "species_category_links",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "species_category_links_species_category_uidx",
                table: "species_category_links",
                columns: new[] { "species_id", "category_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "species_category_links");

            migrationBuilder.DropTable(
                name: "species_categories");
        }
    }
}
