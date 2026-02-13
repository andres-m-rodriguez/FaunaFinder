using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaunaFinder.Wildlife.Database.Migrations
{
    /// <inheritdoc />
    public partial class LocalizeConservationData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use raw SQL with USING clause to convert varchar to jsonb
            // For existing data, wrap the string in a LocaleValue array format
            // For empty/null data, use an empty array
            migrationBuilder.Sql("""
                ALTER TABLE nrcs_practices
                ALTER COLUMN name TYPE jsonb
                USING CASE
                    WHEN name IS NULL OR name = '' THEN '[]'::jsonb
                    ELSE jsonb_build_array(jsonb_build_object('Code', 'en', 'Value', name))
                END;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE fws_actions
                ALTER COLUMN name TYPE jsonb
                USING CASE
                    WHEN name IS NULL OR name = '' THEN '[]'::jsonb
                    ELSE jsonb_build_array(jsonb_build_object('Code', 'en', 'Value', name))
                END;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE fws_links
                ALTER COLUMN justification TYPE jsonb
                USING CASE
                    WHEN justification IS NULL OR justification = '' THEN '[]'::jsonb
                    ELSE jsonb_build_array(jsonb_build_object('Code', 'en', 'Value', justification))
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Extract the English value from the jsonb array back to varchar
            migrationBuilder.Sql("""
                ALTER TABLE nrcs_practices
                ALTER COLUMN name TYPE character varying(500)
                USING (
                    SELECT value->>'Value'
                    FROM jsonb_array_elements(name) AS value
                    WHERE value->>'Code' = 'en'
                    LIMIT 1
                );
                """);

            migrationBuilder.Sql("""
                ALTER TABLE fws_actions
                ALTER COLUMN name TYPE character varying(500)
                USING (
                    SELECT value->>'Value'
                    FROM jsonb_array_elements(name) AS value
                    WHERE value->>'Code' = 'en'
                    LIMIT 1
                );
                """);

            migrationBuilder.Sql("""
                ALTER TABLE fws_links
                ALTER COLUMN justification TYPE character varying(2000)
                USING (
                    SELECT value->>'Value'
                    FROM jsonb_array_elements(justification) AS value
                    WHERE value->>'Code' = 'en'
                    LIMIT 1
                );
                """);
        }
    }
}
