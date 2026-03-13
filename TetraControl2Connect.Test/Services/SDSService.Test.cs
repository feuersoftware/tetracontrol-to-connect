using FeuerSoftware.TetraControl2Connect.Models.Connect;
using FeuerSoftware.TetraControl2Connect.Models.TetraControl;
using FeuerSoftware.TetraControl2Connect.Services;
using FeuerSoftware.TetraControl2Connect.Shared.Options;
using FeuerSoftware.TetraControl2Connect.Shared.Options.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FeuerSoftware.TetraControl2Connect.Test.Services
{
    public class SDSServiceTest
    {
        [Fact]
        public async Task HandleSds_Alarm_GSSI_With_Ignoring_Enabled()
        {
            var number = Guid.NewGuid().ToString();
            var operation = new OperationModel()
            {
                Start = DateTime.Now.AddSeconds(-20),
                CreatedAt = DateTime.Now.AddSeconds(-10),
                Number = number,
            };

            var log = new Mock<ILogger<SDSService>>();
            var connectApiService = new Mock<IConnectApiService>(MockBehavior.Strict);

            var userService = new Mock<IUserService>(MockBehavior.Strict);
            var connectOptions = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptions.Setup(o => o.CurrentValue).Returns(new ConnectOptions());

            var programOptions = new Mock<IOptionsMonitor<ProgramOptions>>(MockBehavior.Strict);
            programOptions.Setup(o => o.CurrentValue).Returns(new ProgramOptions()
            {
                AddPropertyForAlarmTexts = true,
                IgnoreAlarmWithoutSubnetAddresses = true,
                AcceptCalloutsForSirens = false,
            });

            var statusOptions = new Mock<IOptionsMonitor<StatusOptions>>(MockBehavior.Strict);
            statusOptions.Setup(o => o.CurrentValue).Returns(new StatusOptions());

            var severityOptions = new Mock<IOptionsMonitor<SeverityOptions>>(MockBehavior.Strict);
            severityOptions.Setup(o => o.CurrentValue).Returns(new SeverityOptions());

            var sirenCalloutOptions = new Mock<IOptionsMonitor<SirenCalloutOptions>>(MockBehavior.Strict);
            sirenCalloutOptions.Setup(o => o.CurrentValue).Returns(new SirenCalloutOptions());

            var patternOptions = new Mock<IOptionsMonitor<PatternOptions>>(MockBehavior.Strict);
            patternOptions.Setup(o => o.CurrentValue).Returns(new PatternOptions());

            var sitesService = new Mock<ISitesService>();

            var service = new SDSService(
                log.Object,
                connectApiService.Object,
                userService.Object,
                connectOptions.Object,
                programOptions.Object,
                statusOptions.Object,
                sitesService.Object,
                severityOptions.Object,
                sirenCalloutOptions.Object,
                patternOptions.Object);

            var dto = new TetraControlDto()
            {
                Text = "VOLLALARM TEST TEST TEST",
                Remark = "-1;1;124;",
                DestinationSSI = "123456",
            };

            await service.HandleSds(dto);
        }

        [Fact]
        public async Task HandleSds_Alarm_GSSI()
        {
            var accessToken1 = "TEST1";
            var accessToken2 = "TEST2";
            var directAlarms = 0;
            var normalAlarms = 0;
            var operations = new List<OperationModel>()
            {
                new()
                {
                    Start = DateTime.Now.AddSeconds(-20),
                    CreatedAt = DateTime.Now.AddSeconds(-10),
                    Keyword = "XYZ"
                },
            };

            var getOperationCallsCount = 0;

            var log = new Mock<ILogger<SDSService>>();
            var connectApiService = new Mock<IConnectApiService>(MockBehavior.Strict);
            connectApiService.Setup(s => s.GetLatestOperations(accessToken1))
                .ReturnsAsync(operations)
                .Callback(() => getOperationCallsCount++);
            connectApiService.Setup(s => s.PostOperation(
                    accessToken1,
                    It.Is<OperationModel>(o => o.Properties.Count == 1),
                    UpdateStrategy.ByNumber))
                .Returns(Task.CompletedTask)
                .Callback(() => normalAlarms++)
                .Verifiable();

            connectApiService.Setup(s => s.PostOperation(
                    accessToken2,
                    It.Is<OperationModel>(o => o.Keyword == "INFORMATION"),
                    UpdateStrategy.ByNumber))
                .Returns(Task.CompletedTask)
                .Callback(() => directAlarms++)
                .Verifiable();

            var userService = new Mock<IUserService>();
            var connectOptions = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptions.Setup(o => o.CurrentValue).Returns(new ConnectOptions());
            connectOptions.Setup(o => o.CurrentValue.Sites).Returns(
            [
                new Site()
                {
                    Name = "Test1",
                    Key = accessToken1,
                    SubnetAddresses =
                    [
                        new SubnetAddress()
                        {
                            Name = "Test1",
                            SNA = "&21",
                            GSSI = "123456",
                            AlarmDirectly = false,
                        },
                    ],
                },
                new Site()
                {
                    Name = "Test2",
                    Key = accessToken2,
                    SubnetAddresses =
                    [
                        new SubnetAddress()
                        {
                            Name = "Test2",
                            SNA = "&22",
                            GSSI = "123456",
                            AlarmDirectly = true,
                        },
                    ],
                },
                new Site()
                {
                    Name = "Test3",
                    Key = "Test3",
                    SubnetAddresses =
                    [
                        new SubnetAddress()
                        {
                            Name = "Test3",
                            GSSI = "654321",
                            SNA = "&22",
                            AlarmDirectly = true,
                        },
                    ],
                },
            ]);

            var programOptions = new Mock<IOptionsMonitor<ProgramOptions>>();
            programOptions.Setup(o => o.CurrentValue).Returns(new ProgramOptions()
            {
                AddPropertyForAlarmTexts = true,
                AcceptCalloutsForSirens = false,
            });

            var statusOptions = new Mock<IOptionsMonitor<StatusOptions>>();
            statusOptions.Setup(o => o.CurrentValue).Returns(new StatusOptions());

            var severityOptions = new Mock<IOptionsMonitor<SeverityOptions>>(MockBehavior.Strict);
            severityOptions.Setup(o => o.CurrentValue).Returns(new SeverityOptions()
            {
                SeverityTranslations = new Dictionary<string, string>()
                {
                    { "1", "Information" },
                    { "2", "Einsatzabbruch" },
                    { "3", "Bereitschaft" },
                    { "4", "Krankentransport" },
                    { "5", "Rettungsdienst R-0" },
                    { "7", "Hilfeleistung normal" },
                    { "8", "Feuer normal" },
                    { "9", "Rettungsdienst R-1" },
                    { "10", "Rettungsdienst R-2" },
                    { "11", "Hilfeleistung dringend" },
                    { "12", "Feuer dringend" },
                    { "13", "Großalarm" },
                    { "15", "KatS-Alarm" },
                }
            });
            var sirenCalloutOptions = new Mock<IOptionsMonitor<SirenCalloutOptions>>(MockBehavior.Strict);
            sirenCalloutOptions.Setup(o => o.CurrentValue).Returns(new SirenCalloutOptions());

            var patternOptions = new Mock<IOptionsMonitor<PatternOptions>>(MockBehavior.Strict);
            patternOptions.Setup(o => o.CurrentValue).Returns(new PatternOptions());

            var sitesService = new Mock<ISitesService>();

            var service = new SDSService(
                log.Object,
                connectApiService.Object,
                userService.Object,
                connectOptions.Object,
                programOptions.Object,
                statusOptions.Object,
                sitesService.Object,
                severityOptions.Object,
                sirenCalloutOptions.Object,
                patternOptions.Object);

            var dto = new TetraControlDto()
            {
                Text = "VOLLALARM TEST TEST TEST",
                Remark = "-1;1;124;",
                DestinationSSI = "123456",
            };

            await service.HandleSds(dto);

            getOperationCallsCount.Should().Be(1);
            connectApiService.VerifyAll();
            connectApiService.VerifyNoOtherCalls();
            normalAlarms.Should().Be(1);
            directAlarms.Should().Be(1);
        }

        [Fact]
        public async Task HandleSds_Alarm_Update()
        {
            var accessToken1 = "TEST1";
            var accessToken2 = "TEST2";
            var directAlarms = 0;
            var normalAlarms = 0;
            var number = Guid.NewGuid().ToString();
            var operations = new List<OperationModel>()
            {
                new()
                {
                    Start = DateTime.Now.AddSeconds(-20),
                    CreatedAt = DateTime.Now.AddSeconds(-10),
                    Number = number,
                },
            };

            var getOperationCallsCount = 0;

            var log = new Mock<ILogger<SDSService>>();
            var connectApiService = new Mock<IConnectApiService>(MockBehavior.Strict);
            connectApiService.Setup(s => s.GetLatestOperations(accessToken1))
                .ReturnsAsync(operations)
                .Callback(() => getOperationCallsCount++);
            connectApiService.Setup(s => s.PostOperation(
                    accessToken1,
                    It.Is<OperationModel>(o => o.Properties.Count == 1),
                    UpdateStrategy.ByNumber))
                .Returns(Task.CompletedTask)
                .Callback(() => normalAlarms++)
                .Verifiable();

            connectApiService.Setup(s => s.PostOperation(
                    accessToken2,
                    It.Is<OperationModel>(o => o.Keyword == "RETTUNGSDIENST R-0"),
                    UpdateStrategy.ByNumber))
                .Returns(Task.CompletedTask)
                .Callback(() => directAlarms++)
                .Verifiable();

            var userService = new Mock<IUserService>();
            var connectOptions = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptions.Setup(o => o.CurrentValue).Returns(new ConnectOptions());
            connectOptions.Setup(o => o.CurrentValue.Sites).Returns(
            [
                new Site()
                {
                    Name = "Test1",
                    Key = accessToken1,
                    SubnetAddresses =
                    [
                        new SubnetAddress()
                        {
                            GSSI = "123",
                            Name = "Test1",
                            SNA = "&21",
                            AlarmDirectly = false,
                        },
                    ],
                },
                new Site()
                {
                    Name = "Test2",
                    Key = accessToken2,
                    SubnetAddresses =
                    [
                        new SubnetAddress()
                        {
                            GSSI = "123",
                            Name = "Test2",
                            SNA = "&22",
                            AlarmDirectly = true,
                        },
                    ],
                },
            ]);

            var programOptions = new Mock<IOptionsMonitor<ProgramOptions>>();
            programOptions.Setup(o => o.CurrentValue).Returns(new ProgramOptions()
            {
                AddPropertyForAlarmTexts = true,
                AcceptCalloutsForSirens = false,
            });

            var statusOptions = new Mock<IOptionsMonitor<StatusOptions>>();
            statusOptions.Setup(o => o.CurrentValue).Returns(new StatusOptions());

            var severityOptions = new Mock<IOptionsMonitor<SeverityOptions>>(MockBehavior.Strict);
            severityOptions.Setup(o => o.CurrentValue).Returns(new SeverityOptions()
            {
                SeverityTranslations = new Dictionary<string, string>()
                {
                    { "1", "Information" },
                    { "2", "Einsatzabbruch" },
                    { "3", "Bereitschaft" },
                    { "4", "Krankentransport" },
                    { "5", "Rettungsdienst R-0" },
                    { "7", "Hilfeleistung normal" },
                    { "8", "Feuer normal" },
                    { "9", "Rettungsdienst R-1" },
                    { "10", "Rettungsdienst R-2" },
                    { "11", "Hilfeleistung dringend" },
                    { "12", "Feuer dringend" },
                    { "13", "Großalarm" },
                    { "15", "KatS-Alarm" },
                }
            });

            var sirenCalloutOptions = new Mock<IOptionsMonitor<SirenCalloutOptions>>(MockBehavior.Strict);
            sirenCalloutOptions.Setup(o => o.CurrentValue).Returns(new SirenCalloutOptions());

            var patternOptions = new Mock<IOptionsMonitor<PatternOptions>>(MockBehavior.Strict);
            patternOptions.Setup(o => o.CurrentValue).Returns(new PatternOptions());

            var sitesService = new Mock<ISitesService>();

            var service = new SDSService(
                log.Object,
                connectApiService.Object,
                userService.Object,
                connectOptions.Object,
                programOptions.Object,
                statusOptions.Object,
                sitesService.Object,
                severityOptions.Object,
                sirenCalloutOptions.Object,
                patternOptions.Object);

            var dto = new TetraControlDto()
            {
                Text = "&21&22&25&40F BMA - TEST TEST TEST",
                Remark = "-1;5;124;&21&22&25&40",
                DestinationSSI = "123"
            };

            await service.HandleSds(dto);

            getOperationCallsCount.Should().Be(1);
            connectApiService.VerifyAll();
            connectApiService.VerifyNoOtherCalls();
            normalAlarms.Should().Be(1);
            directAlarms.Should().Be(1);
        }

        [Fact]
        public async Task HandleSds_Alarm_Fallback()
        {
            var accessToken = "TEST";
            var getOperationCallsCount = 0;

            var log = new Mock<ILogger<SDSService>>();
            var connectApiService = new Mock<IConnectApiService>(MockBehavior.Strict);
            connectApiService.Setup(s => s.GetLatestOperations(accessToken))
                .Returns(Task.FromResult<IEnumerable<OperationModel>>([]))
                .Callback(() => getOperationCallsCount++);
            connectApiService.Setup(s => s.PostOperation(
                    accessToken,
                    It.Is<OperationModel>(o => o.Keyword == "RETTUNGSDIENST R-0"),
                    UpdateStrategy.ByNumber))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var userService = new Mock<IUserService>();
            var connectOptions = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptions.Setup(o => o.CurrentValue).Returns(new ConnectOptions());
            connectOptions.Setup(o => o.CurrentValue.Sites).Returns(
            [
                new Site()
                {
                    Name = "Test",
                    Key = accessToken,
                    SubnetAddresses =
                    [
                        new SubnetAddress()
                        {
                            GSSI = "123",
                            Name = "Test1",
                            SNA = "&21",
                        },
                    ],
                },
            ]);

            var programOptions = new Mock<IOptionsMonitor<ProgramOptions>>();
            programOptions.Setup(o => o.CurrentValue).Returns(new ProgramOptions()
            {
                PollForActiveOperationBeforeFallbackMaxRetryCount = 2,
                PollForActiveOperationBeforeFallbackDelay = TimeSpan.FromSeconds(1), // Otherwise the test will run 40 seconds
                AcceptCalloutsForSirens = false,
            });

            var statusOptions = new Mock<IOptionsMonitor<StatusOptions>>();
            statusOptions.Setup(o => o.CurrentValue).Returns(new StatusOptions());

            var severityOptions = new Mock<IOptionsMonitor<SeverityOptions>>(MockBehavior.Strict);
            severityOptions.Setup(o => o.CurrentValue).Returns(new SeverityOptions()
            {
                SeverityTranslations = new Dictionary<string, string>()
                {
                    { "1", "Information" },
                    { "2", "Einsatzabbruch" },
                    { "3", "Bereitschaft" },
                    { "4", "Krankentransport" },
                    { "5", "Rettungsdienst R-0" },
                    { "7", "Hilfeleistung normal" },
                    { "8", "Feuer normal" },
                    { "9", "Rettungsdienst R-1" },
                    { "10", "Rettungsdienst R-2" },
                    { "11", "Hilfeleistung dringend" },
                    { "12", "Feuer dringend" },
                    { "13", "Großalarm" },
                    { "15", "KatS-Alarm" },
                }
            });
            var sirenCalloutOptions = new Mock<IOptionsMonitor<SirenCalloutOptions>>(MockBehavior.Strict);
            sirenCalloutOptions.Setup(o => o.CurrentValue).Returns(new SirenCalloutOptions());

            var patternOptions = new Mock<IOptionsMonitor<PatternOptions>>(MockBehavior.Strict);
            patternOptions.Setup(o => o.CurrentValue).Returns(new PatternOptions());

            var sitesService = new Mock<ISitesService>();

            var service = new SDSService(
                log.Object,
                connectApiService.Object,
                userService.Object,
                connectOptions.Object,
                programOptions.Object,
                statusOptions.Object,
                sitesService.Object,
                severityOptions.Object,
                sirenCalloutOptions.Object,
                patternOptions.Object);

            var dto = new TetraControlDto()
            {
                Text = "&21&25&40F BMA - TEST TEST TEST",
                Remark = "-1;5;124;&21&25&40",
                DestinationSSI = "123"
            };

            await service.HandleSds(dto);

            getOperationCallsCount.Should().Be(programOptions.Object.CurrentValue.PollForActiveOperationBeforeFallbackMaxRetryCount + 1);
            connectApiService.VerifyAll();
            connectApiService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task HandleSds_Alarm_No_Update_No_Fallback()
        {
            var accessToken1 = "TEST1";
            var number = Guid.NewGuid().ToString();
            var operations = new List<OperationModel>()
            {
                new()
                {
                    Start = DateTime.Now.AddSeconds(-20),
                    CreatedAt = DateTime.Now.AddSeconds(-10),
                    Number = number,
                },
            };

            var getOperationCallsCount = 0;

            var log = new Mock<ILogger<SDSService>>();
            var connectApiService = new Mock<IConnectApiService>(MockBehavior.Strict);
            connectApiService.Setup(s => s.GetLatestOperations(accessToken1))
                .ReturnsAsync(operations)
                .Callback(() => getOperationCallsCount++)
                .Verifiable();

            var userService = new Mock<IUserService>();
            var connectOptions = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptions.Setup(o => o.CurrentValue).Returns(new ConnectOptions());
            connectOptions.Setup(o => o.CurrentValue.Sites).Returns(
            [
                new Site()
                {
                    Name = "Test1",
                    Key = accessToken1,
                    SubnetAddresses =
                    [
                        new SubnetAddress()
                        {
                            GSSI = "123",
                            Name = "Test1",
                            SNA = "&21",
                            AlarmDirectly = false,
                        },
                    ],
                },
            ]);

            var programOptions = new Mock<IOptionsMonitor<ProgramOptions>>();
            programOptions.Setup(o => o.CurrentValue).Returns(new ProgramOptions()
            {
                AddPropertyForAlarmTexts = true,
                AcceptCalloutsForSirens = false,
                UpdateExistingOperations = false,
            });

            var statusOptions = new Mock<IOptionsMonitor<StatusOptions>>();
            statusOptions.Setup(o => o.CurrentValue).Returns(new StatusOptions());

            var severityOptions = new Mock<IOptionsMonitor<SeverityOptions>>(MockBehavior.Strict);
            severityOptions.Setup(o => o.CurrentValue).Returns(new SeverityOptions()
            {
                SeverityTranslations = new Dictionary<string, string>()
                {
                    { "1", "Information" },
                    { "2", "Einsatzabbruch" },
                    { "3", "Bereitschaft" },
                    { "4", "Krankentransport" },
                    { "5", "Rettungsdienst R-0" },
                    { "7", "Hilfeleistung normal" },
                    { "8", "Feuer normal" },
                    { "9", "Rettungsdienst R-1" },
                    { "10", "Rettungsdienst R-2" },
                    { "11", "Hilfeleistung dringend" },
                    { "12", "Feuer dringend" },
                    { "13", "Großalarm" },
                    { "15", "KatS-Alarm" },
                }
            });
            var sirenCalloutOptions = new Mock<IOptionsMonitor<SirenCalloutOptions>>(MockBehavior.Strict);
            sirenCalloutOptions.Setup(o => o.CurrentValue).Returns(new SirenCalloutOptions());

            var patternOptions = new Mock<IOptionsMonitor<PatternOptions>>(MockBehavior.Strict);
            patternOptions.Setup(o => o.CurrentValue).Returns(new PatternOptions());

            var sitesService = new Mock<ISitesService>();

            var service = new SDSService(
                log.Object,
                connectApiService.Object,
                userService.Object,
                connectOptions.Object,
                programOptions.Object,
                statusOptions.Object,
                sitesService.Object,
                severityOptions.Object,
                sirenCalloutOptions.Object,
                patternOptions.Object);

            var dto = new TetraControlDto()
            {
                Text = "&21&22&25&40F BMA - TEST TEST TEST",
                Remark = "-1;5;124;&21&22&25&40",
                DestinationSSI = "123"
            };

            await service.HandleSds(dto);

            getOperationCallsCount.Should().Be(1);
            connectApiService.VerifyAll();
            connectApiService.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task HandleSds_Alarm_UnknownType_Pattern()
        {
            var accessToken1 = "TEST1";
            var number = Guid.NewGuid().ToString();
            var operations = new List<OperationModel>()
            {
                new()
                {
                    Start = DateTime.Now.AddSeconds(-20),
                    CreatedAt = DateTime.Now.AddSeconds(-10),
                    Number = number,
                },
            };

            OperationModel createdOperation = new();

            var log = new Mock<ILogger<SDSService>>();
            var connectApiService = new Mock<IConnectApiService>(MockBehavior.Strict);
            connectApiService.Setup(s => s.PostOperation(accessToken1, It.IsAny<OperationModel>(), UpdateStrategy.ByNumber))
                .Callback<string, OperationModel, UpdateStrategy>((_, operation, _) => createdOperation = operation)
                .Returns(Task.CompletedTask);

            var userService = new Mock<IUserService>();
            var connectOptions = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptions.Setup(o => o.CurrentValue).Returns(new ConnectOptions()
            {
                Sites =
                [
                    new Site()
                    {
                        Name = "Teststandort1",
                        Key = accessToken1,
                        SubnetAddresses =
                        [
                            new SubnetAddress()
                            {
                                GSSI = "123",
                                SNA = "321",
                                Name = "TestRic1"
                            }
                        ]
                    }
                ]
            });

            var programOptions = new Mock<IOptionsMonitor<ProgramOptions>>();
            programOptions.Setup(o => o.CurrentValue).Returns(new ProgramOptions()
            {
                AcceptSDSAsCalloutsWithPattern = true,
            });

            var statusOptions = new Mock<IOptionsMonitor<StatusOptions>>();
            statusOptions.Setup(o => o.CurrentValue).Returns(new StatusOptions());

            var severityOptions = new Mock<IOptionsMonitor<SeverityOptions>>(MockBehavior.Strict);
            severityOptions.Setup(o => o.CurrentValue).Returns(new SeverityOptions()
            {
                SeverityTranslations = new Dictionary<string, string>()
                {
                    { "1", "Information" },
                    { "2", "Einsatzabbruch" },
                    { "3", "Bereitschaft" },
                    { "4", "Krankentransport" },
                    { "5", "Rettungsdienst R-0" },
                    { "7", "Hilfeleistung normal" },
                    { "8", "Feuer normal" },
                    { "9", "Rettungsdienst R-1" },
                    { "10", "Rettungsdienst R-2" },
                    { "11", "Hilfeleistung dringend" },
                    { "12", "Feuer dringend" },
                    { "13", "Großalarm" },
                    { "15", "KatS-Alarm" },
                }
            });
            var sirenCalloutOptions = new Mock<IOptionsMonitor<SirenCalloutOptions>>(MockBehavior.Strict);
            sirenCalloutOptions.Setup(o => o.CurrentValue).Returns(new SirenCalloutOptions());

            var patternOptions = new Mock<IOptionsMonitor<PatternOptions>>(MockBehavior.Strict);
            patternOptions.Setup(o => o.CurrentValue).Returns(new PatternOptions()
            {
                NumberPattern = "Nummer: ([^\n]*)",
                KeywordPattern = "Stichwort: ([^\n]*)",
                FactsPattern = "Meldebild: ([^\n]*)",
                StreetPattern = "Strasse: ([^\n]*)",
                ZipCodePattern = "PLZ: ([^\n]*)",
                CityPattern = "Ort: ([^\n]*)",
                DistrictPattern = "Ortsteil: ([^\n]*)",
                RicPattern = "RIC: ([^\n]*)",
                HouseNumberPattern = "Hnr: ([^\n]*)",
                AdditionalProperties =
                [
                    new PatternField()
                    {
                        Name = "Hinweis",
                        Pattern = "Hinweis: ([^\n]*)"
                    }
                ]
            });

            var sitesService = new Mock<ISitesService>();

            var service = new SDSService(
                log.Object,
                connectApiService.Object,
                userService.Object,
                connectOptions.Object,
                programOptions.Object,
                statusOptions.Object,
                sitesService.Object,
                severityOptions.Object,
                sirenCalloutOptions.Object,
                patternOptions.Object);

            var dto = new TetraControlDto()
            {
                Text = "Nummer: 1234\r\nStichwort: H1\r\nMeldebild: Test\r\nStrasse: Teststrasse\r\nHnr: 1\r\nPLZ: 12345\r\nOrt: Testort\r\nOrtsteil: Testortsteil\r\nHinweis: Testhinweis\r\nRIC: TestRic1",
            };

            await service.HandleSds(dto);

            createdOperation.Number.Should().Be("1234");
            createdOperation.Facts.Should().Be("Test");
            createdOperation.Address.City.Should().Be("Testort");
            createdOperation.Address.Street.Should().Be("Teststrasse");
            createdOperation.Address.HouseNumber.Should().Be("1");
            createdOperation.Address.District.Should().Be("Testortsteil");
            createdOperation.Properties.Should().HaveCount(1);
        }
    }
}
