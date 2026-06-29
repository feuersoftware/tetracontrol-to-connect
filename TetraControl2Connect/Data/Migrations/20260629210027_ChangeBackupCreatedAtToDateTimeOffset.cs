using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeuerSoftware.TetraControl2Connect.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeBackupCreatedAtToDateTimeOffset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CreatedAt changed from DateTime to DateTimeOffset. The SQLite store type stays TEXT,
            // but existing rows were written without an offset (they hold UTC instants). EF reads
            // an offset-less TEXT value back as local time, so stamp the existing UTC values with
            // an explicit "+00:00" offset to preserve the instant.
            migrationBuilder.Sql(
                """
                UPDATE "SettingsBackups"
                SET "CreatedAt" = "CreatedAt" || '+00:00'
                WHERE "CreatedAt" NOT LIKE '%+%' AND "CreatedAt" NOT LIKE '%Z';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Strip the "+00:00" offset again so the values match the DateTime TEXT format.
            migrationBuilder.Sql(
                """
                UPDATE "SettingsBackups"
                SET "CreatedAt" = substr("CreatedAt", 1, length("CreatedAt") - 6)
                WHERE "CreatedAt" LIKE '%+00:00';
                """);
        }
    }
}
