using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FaunaFinder.Database.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyToSingleProfileImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "species_images");

            migrationBuilder.AddColumn<string>(
                name: "profile_image_content_type",
                table: "species",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "profile_image_data",
                table: "species",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profile_image_content_type",
                table: "species");

            migrationBuilder.DropColumn(
                name: "profile_image_data",
                table: "species");

            migrationBuilder.CreateTable(
                name: "species_images",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    species_id = table.Column<int>(type: "integer", nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    image_data = table.Column<byte[]>(type: "bytea", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_species_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_species_images_species_species_id",
                        column: x => x.species_id,
                        principalTable: "species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "species_images_species_id_idx",
                table: "species_images",
                column: "species_id");

            migrationBuilder.CreateIndex(
                name: "species_images_species_id_is_primary_idx",
                table: "species_images",
                columns: new[] { "species_id", "is_primary" });
        }
    }
}
