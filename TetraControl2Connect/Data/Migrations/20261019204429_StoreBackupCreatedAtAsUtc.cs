using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeuerSoftware.TetraControl2Connect.Data.Migrations
{
    /// <inheritdoc />
    public partial class StoreBackupCreatedAtAsUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normalize existing TEXT values to EF's UTC DateTime format. Convert whole seconds
            // separately so SQLite's millisecond date functions cannot round away .NET ticks.
            migrationBuilder.Sql(
                """
                WITH "BackupTimes" AS (
                    SELECT "Id",
                        CASE WHEN substr("CreatedAt", -6, 1) IN ('+', '-')
                            THEN substr("CreatedAt", 1, length("CreatedAt") - 6)
                            ELSE rtrim("CreatedAt", 'Z')
                        END AS "LocalTime",
                        CASE WHEN substr("CreatedAt", -6, 1) IN ('+', '-')
                            THEN substr("CreatedAt", -6)
                            ELSE '+00:00'
                        END AS "Offset"
                    FROM "SettingsBackups"
                )
                UPDATE "SettingsBackups"
                SET "CreatedAt" = (
                    SELECT strftime('%Y-%m-%d %H:%M:%S', substr("LocalTime", 1, 19) || "Offset")
                        || rtrim(rtrim(substr("LocalTime", 20), '0'), '.')
                    FROM "BackupTimes"
                    WHERE "BackupTimes"."Id" = "SettingsBackups"."Id"
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "SettingsBackups"
                SET "CreatedAt" = "CreatedAt" || '+00:00';
                """);
        }
    }
}
