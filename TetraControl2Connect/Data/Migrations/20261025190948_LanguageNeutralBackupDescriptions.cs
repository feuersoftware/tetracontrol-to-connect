using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeuerSoftware.TetraControl2Connect.Data.Migrations
{
    /// <inheritdoc />
    public partial class LanguageNeutralBackupDescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Replace the German descriptions stored by earlier versions with the language-neutral
            // codes from BackupDescriptions, so the API never returns German text.
            migrationBuilder.Sql(
                """
                UPDATE "SettingsBackups"
                SET "Description" = CASE "Description"
                    WHEN 'Automatische Sicherung' THEN 'automatic'
                    WHEN 'Automatische Sicherung vor Import' THEN 'automatic_before_import'
                    WHEN 'Manuelle Sicherung' THEN 'manual'
                END
                WHERE "Description" IN ('Automatische Sicherung', 'Automatische Sicherung vor Import', 'Manuelle Sicherung');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "SettingsBackups"
                SET "Description" = CASE "Description"
                    WHEN 'automatic' THEN 'Automatische Sicherung'
                    WHEN 'automatic_before_import' THEN 'Automatische Sicherung vor Import'
                    WHEN 'manual' THEN 'Manuelle Sicherung'
                END
                WHERE "Description" IN ('automatic', 'automatic_before_import', 'manual');
                """);
        }
    }
}
