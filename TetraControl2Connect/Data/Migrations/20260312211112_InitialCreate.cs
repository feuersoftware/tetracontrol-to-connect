using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeuerSoftware.TetraControl2Connect.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatternSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumberPattern = table.Column<string>(type: "TEXT", nullable: false),
                    KeywordPattern = table.Column<string>(type: "TEXT", nullable: false),
                    FactsPattern = table.Column<string>(type: "TEXT", nullable: false),
                    ReporterNamePattern = table.Column<string>(type: "TEXT", nullable: false),
                    ReporterPhoneNumberPattern = table.Column<string>(type: "TEXT", nullable: false),
                    CityPattern = table.Column<string>(type: "TEXT", nullable: false),
                    StreetPattern = table.Column<string>(type: "TEXT", nullable: false),
                    HouseNumberPattern = table.Column<string>(type: "TEXT", nullable: false),
                    ZipCodePattern = table.Column<string>(type: "TEXT", nullable: false),
                    DistrictPattern = table.Column<string>(type: "TEXT", nullable: false),
                    LatitudePattern = table.Column<string>(type: "TEXT", nullable: false),
                    LongitudePattern = table.Column<string>(type: "TEXT", nullable: false),
                    RicPattern = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatternSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProgramSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SendVehicleStatus = table.Column<bool>(type: "INTEGER", nullable: false),
                    SendVehiclePositions = table.Column<bool>(type: "INTEGER", nullable: false),
                    SendUserOperationStatus = table.Column<bool>(type: "INTEGER", nullable: false),
                    SendUserAvailability = table.Column<bool>(type: "INTEGER", nullable: false),
                    SendAlarms = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdateExistingOperations = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserAvailabilityLifetimeDays = table.Column<int>(type: "INTEGER", nullable: false),
                    WebSocketReconnectTimeoutMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    PollForActiveOperationBeforeFallbackMaxRetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PollForActiveOperationBeforeFallbackDelay = table.Column<string>(type: "TEXT", nullable: false),
                    HeartbeatEndpointUrl = table.Column<string>(type: "TEXT", nullable: false),
                    HeartbeatInterval = table.Column<string>(type: "TEXT", nullable: true),
                    IgnoreStatus5 = table.Column<bool>(type: "INTEGER", nullable: false),
                    IgnoreStatus0 = table.Column<bool>(type: "INTEGER", nullable: false),
                    IgnoreStatus9 = table.Column<bool>(type: "INTEGER", nullable: false),
                    AddPropertyForAlarmTexts = table.Column<bool>(type: "INTEGER", nullable: false),
                    UseFullyQualifiedSubnetAddressForConnect = table.Column<bool>(type: "INTEGER", nullable: false),
                    IgnoreAlarmWithoutSubnetAddresses = table.Column<bool>(type: "INTEGER", nullable: false),
                    AcceptCalloutsForSirens = table.Column<bool>(type: "INTEGER", nullable: false),
                    AcceptSDSAsCalloutsWithPattern = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SettingsBackups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    SnapshotJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingsBackups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeveritySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UseServerityTranslationAsKeyword = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeveritySettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SirenCalloutSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UseSirenCodeTranslationAsKeyword = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SirenCalloutSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SirenStatusSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SirenStatusSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AvailableStatus = table.Column<string>(type: "TEXT", nullable: false),
                    LimitedAvailableStatus = table.Column<string>(type: "TEXT", nullable: false),
                    NotAvailableStatus = table.Column<string>(type: "TEXT", nullable: false),
                    ComingStatus = table.Column<string>(type: "TEXT", nullable: false),
                    NotComingStatus = table.Column<string>(type: "TEXT", nullable: false),
                    ComingLaterStatus = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TetraControlSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TetraControlHost = table.Column<string>(type: "TEXT", nullable: false),
                    TetraControlPort = table.Column<int>(type: "INTEGER", nullable: false),
                    TetraControlUsername = table.Column<string>(type: "TEXT", nullable: false),
                    TetraControlPassword = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TetraControlSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdditionalPatterns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatternSettingsId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Pattern = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalPatterns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalPatterns_PatternSettings_PatternSettingsId",
                        column: x => x.PatternSettingsId,
                        principalTable: "PatternSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeverityTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SeveritySettingsId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Translation = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeverityTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeverityTranslations_SeveritySettings_SeveritySettingsId",
                        column: x => x.SeveritySettingsId,
                        principalTable: "SeveritySettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SirenCodeTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SirenCalloutSettingsId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Translation = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SirenCodeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SirenCodeTranslations_SirenCalloutSettings_SirenCalloutSettingsId",
                        column: x => x.SirenCalloutSettingsId,
                        principalTable: "SirenCalloutSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FailureTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SirenStatusSettingsId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Translation = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailureTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FailureTranslations_SirenStatusSettings_SirenStatusSettingsId",
                        column: x => x.SirenStatusSettingsId,
                        principalTable: "SirenStatusSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sirens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Issi = table.Column<string>(type: "TEXT", nullable: false),
                    ExpectedHeartbeatInterval = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sirens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sirens_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubnetAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    SNA = table.Column<string>(type: "TEXT", nullable: false),
                    AlarmDirectly = table.Column<bool>(type: "INTEGER", nullable: false),
                    GSSI = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubnetAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubnetAddresses_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalPatterns_PatternSettingsId",
                table: "AdditionalPatterns",
                column: "PatternSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_FailureTranslations_SirenStatusSettingsId",
                table: "FailureTranslations",
                column: "SirenStatusSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_SeverityTranslations_SeveritySettingsId",
                table: "SeverityTranslations",
                column: "SeveritySettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_SirenCodeTranslations_SirenCalloutSettingsId",
                table: "SirenCodeTranslations",
                column: "SirenCalloutSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Sirens_SiteId",
                table: "Sirens",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SubnetAddresses_SiteId",
                table: "SubnetAddresses",
                column: "SiteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdditionalPatterns");

            migrationBuilder.DropTable(
                name: "FailureTranslations");

            migrationBuilder.DropTable(
                name: "ProgramSettings");

            migrationBuilder.DropTable(
                name: "SettingsBackups");

            migrationBuilder.DropTable(
                name: "SeverityTranslations");

            migrationBuilder.DropTable(
                name: "SirenCodeTranslations");

            migrationBuilder.DropTable(
                name: "Sirens");

            migrationBuilder.DropTable(
                name: "StatusSettings");

            migrationBuilder.DropTable(
                name: "SubnetAddresses");

            migrationBuilder.DropTable(
                name: "TetraControlSettings");

            migrationBuilder.DropTable(
                name: "PatternSettings");

            migrationBuilder.DropTable(
                name: "SirenStatusSettings");

            migrationBuilder.DropTable(
                name: "SeveritySettings");

            migrationBuilder.DropTable(
                name: "SirenCalloutSettings");

            migrationBuilder.DropTable(
                name: "Sites");
        }
    }
}
