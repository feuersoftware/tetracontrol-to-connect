using FeuerSoftware.TetraControl2Connect.Shared.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FeuerSoftware.TetraControl2Connect.Data
{
    /// <summary>
    /// Custom configuration provider that reads settings from SQLite relational tables.
    /// New users start with an empty database and configure via the setup wizard.
    /// Existing customers can import settings from appsettings.json using <see cref="ImportFromConfiguration"/>.
    /// </summary>
    public class DatabaseConfigurationProvider : ConfigurationProvider
    {
        private readonly string _connectionString;
        private readonly ILogger? _logger;

        public DatabaseConfigurationProvider(string connectionString, ILogger? logger = null)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public override void Load()
        {
            using var context = CreateContext();
            context.Database.Migrate();

            var hasAnyData = context.ProgramSettings.Any()
                          || context.TetraControlSettings.Any()
                          || context.Sites.Any();

            if (!hasAnyData)
            {
                _logger?.LogInformation("No settings found in database. Will use appsettings.json as fallback.");
                return;
            }

            LoadProgramSettings(context);
            LoadTetraControlSettings(context);
            LoadStatusSettings(context);
            LoadPatternSettings(context);
            LoadSeveritySettings(context);
            LoadSirenCalloutSettings(context);
            LoadSirenStatusSettings(context);
            LoadConnectOptions(context);
        }

        private void LoadProgramSettings(AppDbContext context)
        {
            var entity = context.ProgramSettings.Find(1);
            if (entity is null) return;

            var section = ProgramOptions.SectionName;
            Set($"{section}:SendVehicleStatus", entity.SendVehicleStatus);
            Set($"{section}:SendVehiclePositions", entity.SendVehiclePositions);
            Set($"{section}:SendUserOperationStatus", entity.SendUserOperationStatus);
            Set($"{section}:SendUserAvailability", entity.SendUserAvailability);
            Set($"{section}:SendAlarms", entity.SendAlarms);
            Set($"{section}:UpdateExistingOperations", entity.UpdateExistingOperations);
            Set($"{section}:UserAvailabilityLifetimeDays", entity.UserAvailabilityLifetimeDays);
            Set($"{section}:WebSocketReconnectTimeoutMinutes", entity.WebSocketReconnectTimeoutMinutes);
            Set($"{section}:PollForActiveOperationBeforeFallbackMaxRetryCount", entity.PollForActiveOperationBeforeFallbackMaxRetryCount);
            Set($"{section}:PollForActiveOperationBeforeFallbackDelay", entity.PollForActiveOperationBeforeFallbackDelay);
            Set($"{section}:HeartbeatEndpointUrl", entity.HeartbeatEndpointUrl);
            if (entity.HeartbeatInterval is not null)
                Set($"{section}:HeartbeatInterval", entity.HeartbeatInterval);
            Set($"{section}:IgnoreStatus5", entity.IgnoreStatus5);
            Set($"{section}:IgnoreStatus0", entity.IgnoreStatus0);
            Set($"{section}:IgnoreStatus9", entity.IgnoreStatus9);
            Set($"{section}:AddPropertyForAlarmTexts", entity.AddPropertyForAlarmTexts);
            Set($"{section}:UseFullyQualifiedSubnetAddressForConnect", entity.UseFullyQualifiedSubnetAddressForConnect);
            Set($"{section}:IgnoreAlarmWithoutSubnetAddresses", entity.IgnoreAlarmWithoutSubnetAddresses);
            Set($"{section}:AcceptCalloutsForSirens", entity.AcceptCalloutsForSirens);
            Set($"{section}:AcceptSDSAsCalloutsWithPattern", entity.AcceptSDSAsCalloutsWithPattern);
        }

        private void LoadTetraControlSettings(AppDbContext context)
        {
            var entity = context.TetraControlSettings.Find(1);
            if (entity is null) return;

            var section = TetraControlOptions.SectionName;
            Set($"{section}:TetraControlHost", entity.TetraControlHost);
            Set($"{section}:TetraControlPort", entity.TetraControlPort);
            Set($"{section}:TetraControlUsername", entity.TetraControlUsername);
            Set($"{section}:TetraControlPassword", entity.TetraControlPassword);
        }

        private void LoadStatusSettings(AppDbContext context)
        {
            var entity = context.StatusSettings.Find(1);
            if (entity is null) return;

            var section = StatusOptions.SectionName;
            Set($"{section}:AvailableStatus", entity.AvailableStatus);
            Set($"{section}:LimitedAvailableStatus", entity.LimitedAvailableStatus);
            Set($"{section}:NotAvailableStatus", entity.NotAvailableStatus);
            Set($"{section}:ComingStatus", entity.ComingStatus);
            Set($"{section}:NotComingStatus", entity.NotComingStatus);
            Set($"{section}:ComingLaterStatus", entity.ComingLaterStatus);
        }

        private void LoadPatternSettings(AppDbContext context)
        {
            var entity = context.PatternSettings
                .Include(p => p.AdditionalProperties)
                .FirstOrDefault(p => p.Id == 1);
            if (entity is null) return;

            var section = PatternOptions.SectionName;
            Set($"{section}:NumberPattern", entity.NumberPattern);
            Set($"{section}:KeywordPattern", entity.KeywordPattern);
            Set($"{section}:FactsPattern", entity.FactsPattern);
            Set($"{section}:ReporterNamePattern", entity.ReporterNamePattern);
            Set($"{section}:ReporterPhoneNumberPattern", entity.ReporterPhoneNumberPattern);
            Set($"{section}:CityPattern", entity.CityPattern);
            Set($"{section}:StreetPattern", entity.StreetPattern);
            Set($"{section}:HouseNumberPattern", entity.HouseNumberPattern);
            Set($"{section}:ZipCodePattern", entity.ZipCodePattern);
            Set($"{section}:DistrictPattern", entity.DistrictPattern);
            Set($"{section}:LatitudePattern", entity.LatitudePattern);
            Set($"{section}:LongitudePattern", entity.LongitudePattern);
            Set($"{section}:RicPattern", entity.RicPattern);

            for (var i = 0; i < entity.AdditionalProperties.Count; i++)
            {
                var ap = entity.AdditionalProperties[i];
                Set($"{section}:AdditionalProperties:{i}:Name", ap.Name);
                Set($"{section}:AdditionalProperties:{i}:Pattern", ap.Pattern);
            }
        }

        private void LoadSeveritySettings(AppDbContext context)
        {
            var entity = context.SeveritySettings
                .Include(s => s.SeverityTranslations)
                .FirstOrDefault(s => s.Id == 1);
            if (entity is null) return;

            var section = SeverityOptions.SectionName;
            Set($"{section}:UseServerityTranslationAsKeyword", entity.UseServerityTranslationAsKeyword);

            foreach (var t in entity.SeverityTranslations)
            {
                Set($"{section}:SeverityTranslations:{t.Code}", t.Translation);
            }
        }

        private void LoadSirenCalloutSettings(AppDbContext context)
        {
            var entity = context.SirenCalloutSettings
                .Include(s => s.SirenCodeTranslations)
                .FirstOrDefault(s => s.Id == 1);
            if (entity is null) return;

            var section = SirenCalloutOptions.SectionName;
            Set($"{section}:UseSirenCodeTranslationAsKeyword", entity.UseSirenCodeTranslationAsKeyword);

            foreach (var t in entity.SirenCodeTranslations)
            {
                Set($"{section}:SirenCodeTranslations:{t.Code}", t.Translation);
            }
        }

        private void LoadSirenStatusSettings(AppDbContext context)
        {
            var entity = context.SirenStatusSettings
                .Include(s => s.FailureTranslations)
                .FirstOrDefault(s => s.Id == 1);
            if (entity is null) return;

            var section = SirenStatusOptions.SectionName;

            foreach (var t in entity.FailureTranslations)
            {
                Set($"{section}:FailureTranslations:{t.Code}", t.Translation);
            }
        }

        private void LoadConnectOptions(AppDbContext context)
        {
            var sites = context.Sites
                .Include(s => s.SubnetAddresses)
                .Include(s => s.Sirens)
                .OrderBy(s => s.Id)
                .ToList();

            var section = ConnectOptions.SectionName;

            for (var i = 0; i < sites.Count; i++)
            {
                var site = sites[i];
                var prefix = $"{section}:Sites:{i}";
                Set($"{prefix}:Name", site.Name);
                Set($"{prefix}:Key", site.Key);

                for (var j = 0; j < site.SubnetAddresses.Count; j++)
                {
                    var sa = site.SubnetAddresses[j];
                    var saPrefix = $"{prefix}:SubnetAddresses:{j}";
                    Set($"{saPrefix}:Name", sa.Name);
                    Set($"{saPrefix}:SNA", sa.SNA);
                    Set($"{saPrefix}:AlarmDirectly", sa.AlarmDirectly);
                    Set($"{saPrefix}:GSSI", sa.GSSI);
                }

                for (var j = 0; j < site.Sirens.Count; j++)
                {
                    var siren = site.Sirens[j];
                    var siPrefix = $"{prefix}:Sirens:{j}";
                    Set($"{siPrefix}:Name", siren.Name);
                    Set($"{siPrefix}:Issi", siren.Issi);
                    if (siren.ExpectedHeartbeatInterval is not null)
                        Set($"{siPrefix}:ExpectedHeartbeatInterval", siren.ExpectedHeartbeatInterval);
                }
            }
        }

        private void Set(string key, object? value)
        {
            Data[key] = value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Imports settings from appsettings.json into the database.
        /// Clears all existing settings tables and re-imports from configuration.
        /// Used for migrating existing customer configurations to the database.
        /// Sections not present in appsettings.json are skipped gracefully.
        /// </summary>
        public static async Task ImportFromConfiguration(AppDbContext context, IConfiguration configuration, ILogger? logger = null)
        {
            logger?.LogInformation("Importing settings from appsettings.json...");

            context.FailureTranslations.RemoveRange(context.FailureTranslations);
            context.SirenStatusSettings.RemoveRange(context.SirenStatusSettings);
            context.SirenCodeTranslations.RemoveRange(context.SirenCodeTranslations);
            context.SirenCalloutSettings.RemoveRange(context.SirenCalloutSettings);
            context.SeverityTranslations.RemoveRange(context.SeverityTranslations);
            context.SeveritySettings.RemoveRange(context.SeveritySettings);
            context.AdditionalPatterns.RemoveRange(context.AdditionalPatterns);
            context.PatternSettings.RemoveRange(context.PatternSettings);
            context.StatusSettings.RemoveRange(context.StatusSettings);
            context.SubnetAddresses.RemoveRange(context.SubnetAddresses);
            context.Sirens.RemoveRange(context.Sirens);
            context.Sites.RemoveRange(context.Sites);
            context.TetraControlSettings.RemoveRange(context.TetraControlSettings);
            context.ProgramSettings.RemoveRange(context.ProgramSettings);
            await context.SaveChangesAsync().ConfigureAwait(false);

            var sectionsImported = 0;

            if (ImportSection(SeedProgramSettings, context, configuration, logger)) sectionsImported++;
            if (ImportSection(SeedTetraControlSettings, context, configuration, logger)) sectionsImported++;
            if (ImportSection(SeedStatusSettings, context, configuration, logger)) sectionsImported++;
            if (ImportSection(SeedPatternSettings, context, configuration, logger)) sectionsImported++;
            if (ImportSection(SeedSeveritySettings, context, configuration, logger)) sectionsImported++;
            if (ImportSection(SeedSirenCalloutSettings, context, configuration, logger)) sectionsImported++;
            if (ImportSection(SeedSirenStatusSettings, context, configuration, logger)) sectionsImported++;
            if (ImportSection(SeedConnectOptions, context, configuration, logger)) sectionsImported++;

            await context.SaveChangesAsync().ConfigureAwait(false);

            if (sectionsImported == 0)
                logger?.LogWarning("No settings sections found in appsettings.json. Nothing was imported.");
            else
                logger?.LogInformation($"Import completed. {sectionsImported} section(s) imported from appsettings.json.");
        }

        private static bool ImportSection(Action<AppDbContext, IConfiguration, ILogger?> seedAction, AppDbContext context, IConfiguration configuration, ILogger? logger)
        {
            var countBefore = context.ChangeTracker.Entries().Count(e => e.State == EntityState.Added);
            seedAction(context, configuration, logger);
            var countAfter = context.ChangeTracker.Entries().Count(e => e.State == EntityState.Added);
            return countAfter > countBefore;
        }

        private static void SeedProgramSettings(AppDbContext context, IConfiguration configuration, ILogger? logger)
        {
            var section = configuration.GetSection(ProgramOptions.SectionName);
            if (!section.Exists()) { logger?.LogDebug("Section '{Section}' not found. Skipping.", ProgramOptions.SectionName); return; }

            context.ProgramSettings.Add(new ProgramSettingsEntity
            {
                Id = 1,
                SendVehicleStatus = section.GetValue<bool>(nameof(ProgramSettingsEntity.SendVehicleStatus)),
                SendVehiclePositions = section.GetValue<bool>(nameof(ProgramSettingsEntity.SendVehiclePositions)),
                SendUserOperationStatus = section.GetValue<bool>(nameof(ProgramSettingsEntity.SendUserOperationStatus)),
                SendUserAvailability = section.GetValue<bool>(nameof(ProgramSettingsEntity.SendUserAvailability)),
                SendAlarms = section.GetValue<bool>(nameof(ProgramSettingsEntity.SendAlarms)),
                UpdateExistingOperations = section.GetValue<bool>(nameof(ProgramSettingsEntity.UpdateExistingOperations)),
                UserAvailabilityLifetimeDays = section.GetValue<int>(nameof(ProgramSettingsEntity.UserAvailabilityLifetimeDays)),
                WebSocketReconnectTimeoutMinutes = section.GetValue<int>(nameof(ProgramSettingsEntity.WebSocketReconnectTimeoutMinutes)),
                PollForActiveOperationBeforeFallbackMaxRetryCount = section.GetValue<int>(nameof(ProgramSettingsEntity.PollForActiveOperationBeforeFallbackMaxRetryCount)),
                PollForActiveOperationBeforeFallbackDelay = section.GetValue<string>(nameof(ProgramSettingsEntity.PollForActiveOperationBeforeFallbackDelay)) ?? "00:00:05",
                HeartbeatEndpointUrl = section.GetValue<string>(nameof(ProgramSettingsEntity.HeartbeatEndpointUrl)) ?? string.Empty,
                HeartbeatInterval = section.GetValue<string>(nameof(ProgramSettingsEntity.HeartbeatInterval)),
                IgnoreStatus5 = section.GetValue<bool>(nameof(ProgramSettingsEntity.IgnoreStatus5)),
                IgnoreStatus0 = section.GetValue<bool>(nameof(ProgramSettingsEntity.IgnoreStatus0)),
                IgnoreStatus9 = section.GetValue<bool>(nameof(ProgramSettingsEntity.IgnoreStatus9)),
                AddPropertyForAlarmTexts = section.GetValue<bool>(nameof(ProgramSettingsEntity.AddPropertyForAlarmTexts)),
                UseFullyQualifiedSubnetAddressForConnect = section.GetValue<bool>(nameof(ProgramSettingsEntity.UseFullyQualifiedSubnetAddressForConnect)),
                IgnoreAlarmWithoutSubnetAddresses = section.GetValue<bool>(nameof(ProgramSettingsEntity.IgnoreAlarmWithoutSubnetAddresses)),
                AcceptCalloutsForSirens = section.GetValue<bool>(nameof(ProgramSettingsEntity.AcceptCalloutsForSirens)),
                AcceptSDSAsCalloutsWithPattern = section.GetValue<bool>(nameof(ProgramSettingsEntity.AcceptSDSAsCalloutsWithPattern)),
            });
            logger?.LogDebug("Seeded section '{Section}'.", ProgramOptions.SectionName);
        }

        private static void SeedTetraControlSettings(AppDbContext context, IConfiguration configuration, ILogger? logger)
        {
            var section = configuration.GetSection(TetraControlOptions.SectionName);
            if (!section.Exists()) { logger?.LogDebug("Section '{Section}' not found. Skipping.", TetraControlOptions.SectionName); return; }

            context.TetraControlSettings.Add(new TetraControlSettingsEntity
            {
                Id = 1,
                TetraControlHost = section.GetValue<string>(nameof(TetraControlSettingsEntity.TetraControlHost)) ?? "localhost",
                TetraControlPort = section.GetValue<int>(nameof(TetraControlSettingsEntity.TetraControlPort)),
                TetraControlUsername = section.GetValue<string>(nameof(TetraControlSettingsEntity.TetraControlUsername)) ?? "Connect",
                TetraControlPassword = section.GetValue<string>(nameof(TetraControlSettingsEntity.TetraControlPassword)) ?? "Connect",
            });
            logger?.LogDebug("Seeded section '{Section}'.", TetraControlOptions.SectionName);
        }

        private static void SeedStatusSettings(AppDbContext context, IConfiguration configuration, ILogger? logger)
        {
            var section = configuration.GetSection(StatusOptions.SectionName);
            if (!section.Exists()) { logger?.LogDebug("Section '{Section}' not found. Skipping.", StatusOptions.SectionName); return; }

            context.StatusSettings.Add(new StatusSettingsEntity
            {
                Id = 1,
                AvailableStatus = section.GetValue<string>(nameof(StatusSettingsEntity.AvailableStatus)) ?? string.Empty,
                LimitedAvailableStatus = section.GetValue<string>(nameof(StatusSettingsEntity.LimitedAvailableStatus)) ?? string.Empty,
                NotAvailableStatus = section.GetValue<string>(nameof(StatusSettingsEntity.NotAvailableStatus)) ?? string.Empty,
                ComingStatus = section.GetValue<string>(nameof(StatusSettingsEntity.ComingStatus)) ?? string.Empty,
                NotComingStatus = section.GetValue<string>(nameof(StatusSettingsEntity.NotComingStatus)) ?? string.Empty,
                ComingLaterStatus = section.GetValue<string>(nameof(StatusSettingsEntity.ComingLaterStatus)) ?? string.Empty,
            });
            logger?.LogDebug("Seeded section '{Section}'.", StatusOptions.SectionName);
        }

        private static void SeedPatternSettings(AppDbContext context, IConfiguration configuration, ILogger? logger)
        {
            var section = configuration.GetSection(PatternOptions.SectionName);
            if (!section.Exists()) { logger?.LogDebug("Section '{Section}' not found. Skipping.", PatternOptions.SectionName); return; }

            var entity = new PatternSettingsEntity
            {
                Id = 1,
                NumberPattern = section.GetValue<string>(nameof(PatternSettingsEntity.NumberPattern)) ?? string.Empty,
                KeywordPattern = section.GetValue<string>(nameof(PatternSettingsEntity.KeywordPattern)) ?? string.Empty,
                FactsPattern = section.GetValue<string>(nameof(PatternSettingsEntity.FactsPattern)) ?? string.Empty,
                ReporterNamePattern = section.GetValue<string>(nameof(PatternSettingsEntity.ReporterNamePattern)) ?? string.Empty,
                ReporterPhoneNumberPattern = section.GetValue<string>(nameof(PatternSettingsEntity.ReporterPhoneNumberPattern)) ?? string.Empty,
                CityPattern = section.GetValue<string>(nameof(PatternSettingsEntity.CityPattern)) ?? string.Empty,
                StreetPattern = section.GetValue<string>(nameof(PatternSettingsEntity.StreetPattern)) ?? string.Empty,
                HouseNumberPattern = section.GetValue<string>(nameof(PatternSettingsEntity.HouseNumberPattern)) ?? string.Empty,
                ZipCodePattern = section.GetValue<string>(nameof(PatternSettingsEntity.ZipCodePattern)) ?? string.Empty,
                DistrictPattern = section.GetValue<string>(nameof(PatternSettingsEntity.DistrictPattern)) ?? string.Empty,
                LatitudePattern = section.GetValue<string>(nameof(PatternSettingsEntity.LatitudePattern)) ?? string.Empty,
                LongitudePattern = section.GetValue<string>(nameof(PatternSettingsEntity.LongitudePattern)) ?? string.Empty,
                RicPattern = section.GetValue<string>(nameof(PatternSettingsEntity.RicPattern)) ?? string.Empty,
            };

            var additionalSection = section.GetSection("AdditionalProperties");
            if (additionalSection.Exists())
            {
                foreach (var child in additionalSection.GetChildren())
                {
                    entity.AdditionalProperties.Add(new AdditionalPatternEntity
                    {
                        Name = child.GetValue<string>("Name") ?? string.Empty,
                        Pattern = child.GetValue<string>("Pattern") ?? string.Empty,
                    });
                }
            }

            context.PatternSettings.Add(entity);
            logger?.LogDebug("Seeded section '{Section}'.", PatternOptions.SectionName);
        }

        private static void SeedSeveritySettings(AppDbContext context, IConfiguration configuration, ILogger? logger)
        {
            var section = configuration.GetSection(SeverityOptions.SectionName);
            if (!section.Exists()) { logger?.LogDebug("Section '{Section}' not found. Skipping.", SeverityOptions.SectionName); return; }

            var entity = new SeveritySettingsEntity
            {
                Id = 1,
                UseServerityTranslationAsKeyword = section.GetValue<bool>(nameof(SeveritySettingsEntity.UseServerityTranslationAsKeyword)),
            };

            var translationsSection = section.GetSection("SeverityTranslations");
            if (translationsSection.Exists())
            {
                foreach (var child in translationsSection.GetChildren())
                {
                    entity.SeverityTranslations.Add(new SeverityTranslationEntity
                    {
                        Code = child.Key,
                        Translation = child.Value ?? string.Empty,
                    });
                }
            }

            context.SeveritySettings.Add(entity);
            logger?.LogDebug("Seeded section '{Section}'.", SeverityOptions.SectionName);
        }

        private static void SeedSirenCalloutSettings(AppDbContext context, IConfiguration configuration, ILogger? logger)
        {
            var section = configuration.GetSection(SirenCalloutOptions.SectionName);
            if (!section.Exists()) { logger?.LogDebug("Section '{Section}' not found. Skipping.", SirenCalloutOptions.SectionName); return; }

            var entity = new SirenCalloutSettingsEntity
            {
                Id = 1,
                UseSirenCodeTranslationAsKeyword = section.GetValue<bool>(nameof(SirenCalloutSettingsEntity.UseSirenCodeTranslationAsKeyword)),
            };

            var translationsSection = section.GetSection("SirenCodeTranslations");
            if (translationsSection.Exists())
            {
                foreach (var child in translationsSection.GetChildren())
                {
                    entity.SirenCodeTranslations.Add(new SirenCodeTranslationEntity
                    {
                        Code = child.Key,
                        Translation = child.Value ?? string.Empty,
                    });
                }
            }

            context.SirenCalloutSettings.Add(entity);
            logger?.LogDebug("Seeded section '{Section}'.", SirenCalloutOptions.SectionName);
        }

        private static void SeedSirenStatusSettings(AppDbContext context, IConfiguration configuration, ILogger? logger)
        {
            var section = configuration.GetSection(SirenStatusOptions.SectionName);
            if (!section.Exists()) { logger?.LogDebug("Section '{Section}' not found. Skipping.", SirenStatusOptions.SectionName); return; }

            var entity = new SirenStatusSettingsEntity { Id = 1 };

            var translationsSection = section.GetSection("FailureTranslations");
            if (translationsSection.Exists())
            {
                foreach (var child in translationsSection.GetChildren())
                {
                    entity.FailureTranslations.Add(new FailureTranslationEntity
                    {
                        Code = child.Key,
                        Translation = child.Value ?? string.Empty,
                    });
                }
            }

            context.SirenStatusSettings.Add(entity);
            logger?.LogDebug("Seeded section '{Section}'.", SirenStatusOptions.SectionName);
        }

        private static void SeedConnectOptions(AppDbContext context, IConfiguration configuration, ILogger? logger)
        {
            var section = configuration.GetSection(ConnectOptions.SectionName);
            if (!section.Exists()) { logger?.LogDebug("Section '{Section}' not found. Skipping.", ConnectOptions.SectionName); return; }

            var sitesSection = section.GetSection("Sites");
            if (!sitesSection.Exists()) return;

            foreach (var siteSection in sitesSection.GetChildren())
            {
                var siteEntity = new SiteEntity
                {
                    Name = siteSection.GetValue<string>("Name") ?? string.Empty,
                    Key = siteSection.GetValue<string>("Key") ?? string.Empty,
                };

                var subnetSection = siteSection.GetSection("SubnetAddresses");
                if (subnetSection.Exists())
                {
                    foreach (var saChild in subnetSection.GetChildren())
                    {
                        siteEntity.SubnetAddresses.Add(new SubnetAddressEntity
                        {
                            Name = saChild.GetValue<string>("Name") ?? string.Empty,
                            SNA = saChild.GetValue<string>("SNA") ?? string.Empty,
                            AlarmDirectly = saChild.GetValue<bool>("AlarmDirectly"),
                            GSSI = saChild.GetValue<string>("GSSI") ?? string.Empty,
                        });
                    }
                }

                var sirensSection = siteSection.GetSection("Sirens");
                if (sirensSection.Exists())
                {
                    foreach (var siChild in sirensSection.GetChildren())
                    {
                        siteEntity.Sirens.Add(new SirenEntity
                        {
                            Name = siChild.GetValue<string>("Name") ?? string.Empty,
                            Issi = siChild.GetValue<string>("Issi") ?? string.Empty,
                            ExpectedHeartbeatInterval = siChild.GetValue<string>("ExpectedHeartbeatInterval"),
                        });
                    }
                }

                context.Sites.Add(siteEntity);
            }

            logger?.LogDebug("Seeded section '{Section}'.", ConnectOptions.SectionName);
        }

        /// <summary>
        /// Seeds the database with sensible defaults for a fresh installation.
        /// These match the original appsettings.json template that shipped with the application.
        /// </summary>
        public static async Task SeedDefaults(AppDbContext context, ILogger? logger = null)
        {
            logger?.LogInformation("Seeding database with default settings for fresh installation...");

            context.ProgramSettings.Add(new ProgramSettingsEntity
            {
                Id = 1,
                SendVehicleStatus = true,
                SendVehiclePositions = true,
                SendUserOperationStatus = true,
                SendUserAvailability = true,
                SendAlarms = true,
                UpdateExistingOperations = true,
                UserAvailabilityLifetimeDays = 7,
                WebSocketReconnectTimeoutMinutes = 5,
                PollForActiveOperationBeforeFallbackMaxRetryCount = 4,
                PollForActiveOperationBeforeFallbackDelay = "00:00:10",
                HeartbeatEndpointUrl = string.Empty,
                HeartbeatInterval = "00:10:00",
            });

            context.TetraControlSettings.Add(new TetraControlSettingsEntity
            {
                Id = 1,
                TetraControlHost = "localhost",
                TetraControlPort = 8085,
                TetraControlUsername = "Connect",
                TetraControlPassword = "Connect",
            });

            context.StatusSettings.Add(new StatusSettingsEntity
            {
                Id = 1,
                AvailableStatus = "15",
                LimitedAvailableStatus = "-1",
                NotAvailableStatus = "0",
                ComingStatus = "32768;57345",
                NotComingStatus = "32769;57344",
                ComingLaterStatus = "-1",
            });

            context.PatternSettings.Add(new PatternSettingsEntity { Id = 1 });

            context.SeveritySettings.Add(new SeveritySettingsEntity
            {
                Id = 1,
                UseServerityTranslationAsKeyword = true,
                SeverityTranslations =
                [
                    new() { SeveritySettingsId = 1, Code = "1", Translation = "Information" },
                    new() { SeveritySettingsId = 1, Code = "2", Translation = "Einsatzabbruch" },
                    new() { SeveritySettingsId = 1, Code = "3", Translation = "Bereitschaft" },
                    new() { SeveritySettingsId = 1, Code = "4", Translation = "Krankentransport" },
                    new() { SeveritySettingsId = 1, Code = "5", Translation = "Rettungsdienst R-0" },
                    new() { SeveritySettingsId = 1, Code = "7", Translation = "Hilfeleistung normal" },
                    new() { SeveritySettingsId = 1, Code = "8", Translation = "Feuer normal" },
                    new() { SeveritySettingsId = 1, Code = "9", Translation = "Rettungsdienst R-1" },
                    new() { SeveritySettingsId = 1, Code = "10", Translation = "Rettungsdienst R-2" },
                    new() { SeveritySettingsId = 1, Code = "11", Translation = "Hilfeleistung dringend" },
                    new() { SeveritySettingsId = 1, Code = "12", Translation = "Feuer dringend" },
                    new() { SeveritySettingsId = 1, Code = "13", Translation = "Großalarm" },
                    new() { SeveritySettingsId = 1, Code = "15", Translation = "KatS-Alarm" },
                ],
            });

            context.SirenCalloutSettings.Add(new SirenCalloutSettingsEntity
            {
                Id = 1,
                UseSirenCodeTranslationAsKeyword = false,
                SirenCodeTranslations =
                [
                    new() { SirenCalloutSettingsId = 1, Code = "$2000", Translation = "Warnung der Bevölkerung" },
                    new() { SirenCalloutSettingsId = 1, Code = "$2001", Translation = "Entwarnung" },
                    new() { SirenCalloutSettingsId = 1, Code = "$2002", Translation = "Feueralarm" },
                    new() { SirenCalloutSettingsId = 1, Code = "$2003", Translation = "Probealarm" },
                ],
            });

            context.SirenStatusSettings.Add(new SirenStatusSettingsEntity { Id = 1 });

            await context.SaveChangesAsync().ConfigureAwait(false);
            logger?.LogInformation("Default settings seeded successfully.");
        }

        private AppDbContext CreateContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite(_connectionString);
            return new AppDbContext(optionsBuilder.Options);
        }
    }

    public class DatabaseConfigurationSource(string connectionString) : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new DatabaseConfigurationProvider(connectionString);
        }
    }

    public static class DatabaseConfigurationExtensions
    {
        public static IConfigurationBuilder AddDatabaseConfiguration(this IConfigurationBuilder builder, string connectionString)
        {
            builder.Add(new DatabaseConfigurationSource(connectionString));
            return builder;
        }
    }
}
