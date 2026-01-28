using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FaunaFinder.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddWildlifeDiscoveryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_species",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    common_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    scientific_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    photo_data = table.Column<byte[]>(type: "bytea", nullable: true),
                    photo_content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    is_endangered = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    verified_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    approved_species_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_species", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_species_species_approved_species_id",
                        column: x => x.approved_species_id,
                        principalTable: "species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_user_species_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_species_users_verified_by_user_id",
                        column: x => x.verified_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sightings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    species_id = table.Column<int>(type: "integer", nullable: true),
                    user_species_id = table.Column<int>(type: "integer", nullable: true),
                    mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    confidence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    count = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    behaviors = table.Column<int>(type: "integer", nullable: false),
                    evidence = table.Column<int>(type: "integer", nullable: false),
                    weather = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    location = table.Column<Point>(type: "geometry(Point, 4326)", nullable: false),
                    municipality_id = table.Column<int>(type: "integer", nullable: true),
                    observed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    photo_data = table.Column<byte[]>(type: "bytea", nullable: true),
                    photo_content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    audio_data = table.Column<byte[]>(type: "bytea", nullable: true),
                    audio_content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_flagged_for_review = table.Column<bool>(type: "boolean", nullable: false),
                    is_new_municipality_record = table.Column<bool>(type: "boolean", nullable: false),
                    review_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewed_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    reported_by_user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sightings", x => x.id);
                    table.CheckConstraint("CK_sightings_species_reference", "species_id IS NOT NULL OR user_species_id IS NOT NULL");
                    table.ForeignKey(
                        name: "fk_sightings_municipalities_municipality_id",
                        column: x => x.municipality_id,
                        principalTable: "municipalities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_sightings_species_species_id",
                        column: x => x.species_id,
                        principalTable: "species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sightings_user_species_user_species_id",
                        column: x => x.user_species_id,
                        principalTable: "user_species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sightings_users_reported_by_user_id",
                        column: x => x.reported_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sightings_users_reviewed_by_user_id",
                        column: x => x.reviewed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sightings_municipality_id",
                table: "sightings",
                column: "municipality_id");

            migrationBuilder.CreateIndex(
                name: "ix_sightings_reviewed_by_user_id",
                table: "sightings",
                column: "reviewed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_sightings_species_id",
                table: "sightings",
                column: "species_id");

            migrationBuilder.CreateIndex(
                name: "ix_sightings_user_species_id",
                table: "sightings",
                column: "user_species_id");

            migrationBuilder.CreateIndex(
                name: "sightings_is_flagged_for_review_idx",
                table: "sightings",
                column: "is_flagged_for_review");

            migrationBuilder.CreateIndex(
                name: "sightings_location_gist_idx",
                table: "sightings",
                column: "location")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "sightings_observed_at_idx",
                table: "sightings",
                column: "observed_at");

            migrationBuilder.CreateIndex(
                name: "sightings_reported_by_user_id_idx",
                table: "sightings",
                column: "reported_by_user_id");

            migrationBuilder.CreateIndex(
                name: "sightings_status_idx",
                table: "sightings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_user_species_approved_species_id",
                table: "user_species",
                column: "approved_species_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_species_created_by_user_id",
                table: "user_species",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_species_verified_by_user_id",
                table: "user_species",
                column: "verified_by_user_id");

            migrationBuilder.CreateIndex(
                name: "user_species_common_name_idx",
                table: "user_species",
                column: "common_name");

            migrationBuilder.CreateIndex(
                name: "user_species_is_verified_idx",
                table: "user_species",
                column: "is_verified");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sightings");

            migrationBuilder.DropTable(
                name: "user_species");
        }
    }
}
