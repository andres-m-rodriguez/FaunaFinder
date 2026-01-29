using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FaunaFinder.Wildlife.Database.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fws_actions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fws_actions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "municipalities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    geo_json_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    boundary = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_municipalities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nrcs_practices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nrcs_practices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "municipality_species",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    municipality_id = table.Column<int>(type: "integer", nullable: false),
                    species_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_municipality_species", x => x.id);
                    table.ForeignKey(
                        name: "fk_municipality_species_municipalities_municipality_id",
                        column: x => x.municipality_id,
                        principalTable: "municipalities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_municipality_species_species_species_id",
                        column: x => x.species_id,
                        principalTable: "species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fws_links",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nrcs_practice_id = table.Column<int>(type: "integer", nullable: false),
                    fws_action_id = table.Column<int>(type: "integer", nullable: false),
                    species_id = table.Column<int>(type: "integer", nullable: false),
                    justification = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fws_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_fws_links_fws_actions_fws_action_id",
                        column: x => x.fws_action_id,
                        principalTable: "fws_actions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_fws_links_nrcs_practices_nrcs_practice_id",
                        column: x => x.nrcs_practice_id,
                        principalTable: "nrcs_practices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_fws_links_species_species_id",
                        column: x => x.species_id,
                        principalTable: "species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sightings_municipality_id",
                table: "sightings",
                column: "municipality_id");

            migrationBuilder.CreateIndex(
                name: "fws_actions_code_uidx",
                table: "fws_actions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "fws_links_practice_action_species_idx",
                table: "fws_links",
                columns: new[] { "nrcs_practice_id", "fws_action_id", "species_id" });

            migrationBuilder.CreateIndex(
                name: "ix_fws_links_fws_action_id",
                table: "fws_links",
                column: "fws_action_id");

            migrationBuilder.CreateIndex(
                name: "ix_fws_links_species_id",
                table: "fws_links",
                column: "species_id");

            migrationBuilder.CreateIndex(
                name: "municipalities_boundary_gist_idx",
                table: "municipalities",
                column: "boundary")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "municipalities_geojson_id_uidx",
                table: "municipalities",
                column: "geo_json_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "municipalities_name_uidx",
                table: "municipalities",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_municipality_species_species_id",
                table: "municipality_species",
                column: "species_id");

            migrationBuilder.CreateIndex(
                name: "municipality_species_municipality_species_uidx",
                table: "municipality_species",
                columns: new[] { "municipality_id", "species_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "nrcs_practices_code_uidx",
                table: "nrcs_practices",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_sightings_municipalities_municipality_id",
                table: "sightings",
                column: "municipality_id",
                principalTable: "municipalities",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sightings_municipalities_municipality_id",
                table: "sightings");

            migrationBuilder.DropTable(
                name: "fws_links");

            migrationBuilder.DropTable(
                name: "municipality_species");

            migrationBuilder.DropTable(
                name: "fws_actions");

            migrationBuilder.DropTable(
                name: "nrcs_practices");

            migrationBuilder.DropTable(
                name: "municipalities");

            migrationBuilder.DropIndex(
                name: "ix_sightings_municipality_id",
                table: "sightings");
        }
    }
}
