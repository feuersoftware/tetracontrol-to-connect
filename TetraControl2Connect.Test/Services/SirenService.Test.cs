using Bogus;
using FeuerSoftware.TetraControl2Connect.Models.Connect;
using FeuerSoftware.TetraControl2Connect.Models.TetraControl;
using FeuerSoftware.TetraControl2Connect.Services;
using FeuerSoftware.TetraControl2Connect.Shared.Options;
using FeuerSoftware.TetraControl2Connect.Shared.Options.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FeuerSoftware.TetraControl2Connect.Test.Services
{
    public class SirenServiceTest
    {
        private static readonly Faker Faker = new();

        [Fact]
        public async Task HandleSirenStatuscode_With_Error_Code_Single()
        {
            var issi = "1234567";
            var name = "Testsirene 1";
            var key = Faker.Random.AlphaNumeric(500);
            var siteId = Faker.Random.Int(0, 5000);

            var dto = new TetraControlDto
            {
                Type = "status",
                Status = string.Empty,
                StatusCode = "E007",
                StatusText = "E007",
                SourceSSI = "1234567",
            };

            var log = new Mock<ILogger<SirenService>>();
            var sirenLog = new Mock<Serilog.ILogger>();
            var connectApiService = new Mock<IConnectApiService>(MockBehavior.Strict);
            connectApiService
                .Setup(s => s.GetOrganizationInfo(key))
                .ReturnsAsync(new OrganizationModel { Id = 1, Name = "Testorga", Sites = [new SiteModel { Name = "Test1", Id = siteId }] })
                .Verifiable();
            connectApiService
                .Setup(s => s.GetDefectReports(key))
                .ReturnsAsync([])
                .Verifiable();
            connectApiService
                .Setup(s => s.GetDefectReportCategories(key))
                .ReturnsAsync([])
                .Verifiable();
            connectApiService
                .Setup(s => s.PostDefectReportCategory(It.Is<DefectReportCategoryModel>(x => x.Name == "TETRA-Sirenen"), key))
                .Returns(Task.CompletedTask)
                .Verifiable();
            connectApiService
                .Setup(s => s.PostDefectReport(It.Is<DefectReportModel>(d => d.SiteId == siteId && d.ShortDescription.Contains("Sabotagealarm")), key))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var connectOptions = new Mock<IOptions<ConnectOptions>>();
            connectOptions.Setup(o => o.Value).Returns(new ConnectOptions
            {
                Sites = [
                    new()
                    {
                        Name = "Test1",
                        Sirens = [ new Siren() { Name = name, Issi = issi }],
                        Key = key,
                    }
                ]
            });

            var sirenStatusOptions = new Mock<IOptions<SirenStatusOptions>>();
            sirenStatusOptions.Setup(o => o.Value).Returns(new SirenStatusOptions()
            {
                FailureTranslations = new Dictionary<string, string>()
                {
                    { "E001", "Nicht ausgelöst, Sirene hat auf eine Alarmierung nicht ausgelöst" },
                    { "E003", "Alarmierung: Besetzt und abgelehnt, Sirene war zum Zeitpunkt der Alarmierung mit einem anderen Auftrag belegt." },
                    { "E005", "Technischer Status Fehler, Sirene nicht für Alarmierung verfügbar" },
                    { "E006", "Sirene temporär abgeschaltet, Sirene steht nicht für Alarmierungen zur Verfügung" },
                    { "E007", "Sabotagealarm, Türkontakt geöffnet" },
                    { "E008", "Fehler Netzstromversorgung" },
                    { "E009", "Fehler Batteriestromversorgung" },
                    { "E00A", "Übertemperatur (Überhitzung, Brand)" },
                    { "Fehler bei der Alarmauslösung", "Nicht ausgelöst, Sirene hat auf eine Alarmierung nicht ausgelöst" },
                    { "STATUS=1", "Technischer Status Fehler (allgemein)" },
                    { "SPRT Sabotage: geöffnet", "Sabotagealarm, Türkontakt geöffnet" },
                    { "SPRT Fehler Netz, Batteriebetrieb", "Fehler Netzstromversorgung"},
                    { "SPRT Batteriespannung niedrig", "Fehler Batteriestromversorgung" },
                    { "SPRT Temperatur zu hoch!", "Übertemperatur (Überhitzung, Brand)" },
                    { "SPRT Sammelstörung", "Sammelstörung, nicht näher bezeichnet" }
                }
            });

            var sirenService = new SirenService(log.Object, connectApiService.Object, connectOptions.Object, sirenStatusOptions.Object, sirenLog.Object);

            await sirenService.HandleSirenStatuscode(dto);

            connectApiService.VerifyAll();
        }

        [Fact]
        public async Task HandleSirenStatuscode_With_Error_Code_Double()
        {
            var issi = "1234567";
            var name = "Testsirene 1";
            var key = Faker.Random.AlphaNumeric(500);
            var siteId = Faker.Random.Int(0, 5000);

            var dto = new TetraControlDto
            {
                Type = "status",
                Status = string.Empty,
                StatusCode = "E007",
                StatusText = "E007",
                SourceSSI = "1234567",
            };

            var log = new Mock<ILogger<SirenService>>();
            var sirenLog = new Mock<Serilog.ILogger>();
            var connectApiService = new Mock<IConnectApiService>(MockBehavior.Strict);
            connectApiService
                .Setup(s => s.GetOrganizationInfo(key))
                .ReturnsAsync(new OrganizationModel { Id = 1, Name = "Testorga", Sites = [new SiteModel { Name = "Test1", Id = siteId }] })
                .Verifiable();
            connectApiService
                .Setup(s => s.GetDefectReports(key))
                .ReturnsAsync(
                [
                    new DefectReportModel { Id = 1, ShortDescription = $"Sirenen-Störung: {name} | Sabotagealarm, Türkontakt geöffnet", CreatedAt = DateTime.Now.AddDays(-6)}
                ])
                .Verifiable();

            var connectOptions = new Mock<IOptions<ConnectOptions>>();
            connectOptions.Setup(o => o.Value).Returns(new ConnectOptions
            {
                Sites = [
                    new()
                    {
                        Name = "Test1",
                        Sirens = [ new Siren() { Name = name, Issi = issi }],
                        Key = key,
                    }
                ]
            });

            var sirenStatusOptions = new Mock<IOptions<SirenStatusOptions>>();
            sirenStatusOptions.Setup(o => o.Value).Returns(new SirenStatusOptions()
            {
                FailureTranslations = new Dictionary<string, string>()
                {
                    { "E001", "Nicht ausgelöst, Sirene hat auf eine Alarmierung nicht ausgelöst" },
                    { "E003", "Alarmierung: Besetzt und abgelehnt, Sirene war zum Zeitpunkt der Alarmierung mit einem anderen Auftrag belegt." },
                    { "E005", "Technischer Status Fehler, Sirene nicht für Alarmierung verfügbar" },
                    { "E006", "Sirene temporär abgeschaltet, Sirene steht nicht für Alarmierungen zur Verfügung" },
                    { "E007", "Sabotagealarm, Türkontakt geöffnet" },
                    { "E008", "Fehler Netzstromversorgung" },
                    { "E009", "Fehler Batteriestromversorgung" },
                    { "E00A", "Übertemperatur (Überhitzung, Brand)" },
                    { "Fehler bei der Alarmauslösung", "Nicht ausgelöst, Sirene hat auf eine Alarmierung nicht ausgelöst" },
                    { "STATUS=1", "Technischer Status Fehler (allgemein)" },
                    { "SPRT Sabotage: geöffnet", "Sabotagealarm, Türkontakt geöffnet" },
                    { "SPRT Fehler Netz, Batteriebetrieb", "Fehler Netzstromversorgung"},
                    { "SPRT Batteriespannung niedrig", "Fehler Batteriestromversorgung" },
                    { "SPRT Temperatur zu hoch!", "Übertemperatur (Überhitzung, Brand)" },
                    { "SPRT Sammelstörung", "Sammelstörung, nicht näher bezeichnet" }
                }
            });
            var sirenService = new SirenService(log.Object, connectApiService.Object, connectOptions.Object, sirenStatusOptions.Object, sirenLog.Object);

            await sirenService.HandleSirenStatuscode(dto);

            connectApiService.VerifyAll();
            connectApiService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task HandleSirenStatuscode_UnknownStatusCode_SkipsProcessing()
        {
            var dto = new TetraControlDto
            {
                Type = "status",
                Status = string.Empty,
                StatusCode = "UNKNOWN",
                StatusText = "UNKNOWN",
                Text = "some unknown text",
                SourceSSI = "1234567",
            };

            var (sirenService, connectApiService) = CreateSirenService();

            await sirenService.HandleSirenStatuscode(dto);

            connectApiService.Verify(s => s.GetOrganizationInfo(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task HandleSirenStatuscode_TextContainsErrorKey_MatchesByText()
        {
            var issi = "1234567";
            var name = "Testsirene 1";
            var key = Faker.Random.AlphaNumeric(100);
            var siteId = Faker.Random.Int(0, 5000);

            var dto = new TetraControlDto
            {
                Type = "status",
                Status = string.Empty,
                StatusCode = string.Empty,
                StatusText = string.Empty,
                Text = "SPRT Sabotage: geöffnet",
                SourceSSI = issi,
            };

            var connectApiService = new Mock<IConnectApiService>();
            connectApiService
                .Setup(s => s.GetOrganizationInfo(key))
                .ReturnsAsync(new OrganizationModel { Id = 1, Name = "Testorga", Sites = [new SiteModel { Name = "Test1", Id = siteId }] });
            connectApiService
                .Setup(s => s.GetDefectReports(key))
                .ReturnsAsync([]);
            connectApiService
                .Setup(s => s.GetDefectReportCategories(key))
                .ReturnsAsync([new DefectReportCategoryModel { Name = "TETRA-Sirenen", SiteId = siteId, Id = 1 }]);
            connectApiService
                .Setup(s => s.PostDefectReport(It.IsAny<DefectReportModel>(), key))
                .Returns(Task.CompletedTask);

            var connectOptions = new Mock<IOptions<ConnectOptions>>();
            connectOptions.Setup(o => o.Value).Returns(new ConnectOptions
            {
                Sites = [new() { Name = "Test1", Sirens = [new Siren() { Name = name, Issi = issi }], Key = key }]
            });

            var sirenService = CreateSirenServiceWithDeps(connectApiService.Object, connectOptions.Object);

            await sirenService.HandleSirenStatuscode(dto);

            connectApiService.Verify(s => s.PostDefectReport(
                It.Is<DefectReportModel>(d => d.ShortDescription.Contains("Sabotagealarm")),
                key), Times.Once);
        }

        [Fact]
        public async Task HandleSirenStatuscode_MultipleSites_CreatesDefectReportForEach()
        {
            var issi = "1234567";
            var name = "Testsirene 1";
            var key1 = Faker.Random.AlphaNumeric(100);
            var key2 = Faker.Random.AlphaNumeric(100);

            var dto = new TetraControlDto
            {
                Type = "status",
                StatusCode = "E008",
                Text = string.Empty,
                SourceSSI = issi,
            };

            var connectApiService = new Mock<IConnectApiService>();
            connectApiService
                .Setup(s => s.GetOrganizationInfo(It.IsAny<string>()))
                .ReturnsAsync(new OrganizationModel { Id = 1, Name = "Org", Sites = [new SiteModel { Id = 1, Name = "Site" }] });
            connectApiService
                .Setup(s => s.GetDefectReports(It.IsAny<string>()))
                .ReturnsAsync([]);
            connectApiService
                .Setup(s => s.GetDefectReportCategories(It.IsAny<string>()))
                .ReturnsAsync([new DefectReportCategoryModel { Name = "TETRA-Sirenen", SiteId = 1, Id = 1 }]);
            connectApiService
                .Setup(s => s.PostDefectReport(It.IsAny<DefectReportModel>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var connectOptions = new Mock<IOptions<ConnectOptions>>();
            connectOptions.Setup(o => o.Value).Returns(new ConnectOptions
            {
                Sites = [
                    new() { Name = "Site1", Sirens = [new Siren() { Name = name, Issi = issi }], Key = key1 },
                    new() { Name = "Site2", Sirens = [new Siren() { Name = name, Issi = issi }], Key = key2 },
                ]
            });

            var sirenService = CreateSirenServiceWithDeps(connectApiService.Object, connectOptions.Object);

            await sirenService.HandleSirenStatuscode(dto);

            connectApiService.Verify(s => s.PostDefectReport(It.IsAny<DefectReportModel>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task HandleSirenStatuscode_SirenNotConfigured_SkipsSite()
        {
            var dto = new TetraControlDto
            {
                Type = "status",
                StatusCode = "E008",
                Text = string.Empty,
                SourceSSI = "9999999",
            };

            var (sirenService, connectApiService) = CreateSirenService();

            await sirenService.HandleSirenStatuscode(dto);

            connectApiService.Verify(s => s.GetOrganizationInfo(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task HandleSirenStatuscode_ExistingCategoryFound_DoesNotCreateCategory()
        {
            var issi = "1234567";
            var key = Faker.Random.AlphaNumeric(100);
            var siteId = 42;

            var dto = new TetraControlDto
            {
                Type = "status",
                StatusCode = "E001",
                Text = string.Empty,
                SourceSSI = issi,
            };

            var connectApiService = new Mock<IConnectApiService>();
            connectApiService
                .Setup(s => s.GetOrganizationInfo(key))
                .ReturnsAsync(new OrganizationModel { Id = 1, Name = "Org", Sites = [new SiteModel { Id = siteId, Name = "Site" }] });
            connectApiService
                .Setup(s => s.GetDefectReports(key)).ReturnsAsync([]);
            connectApiService
                .Setup(s => s.GetDefectReportCategories(key))
                .ReturnsAsync([new DefectReportCategoryModel { Name = "TETRA-Sirenen", SiteId = siteId, Id = 5 }]);
            connectApiService
                .Setup(s => s.PostDefectReport(It.IsAny<DefectReportModel>(), key))
                .Returns(Task.CompletedTask);

            var connectOptions = new Mock<IOptions<ConnectOptions>>();
            connectOptions.Setup(o => o.Value).Returns(new ConnectOptions
            {
                Sites = [new() { Name = "Site", Sirens = [new Siren() { Name = "Sirene", Issi = issi }], Key = key }]
            });

            var sirenService = CreateSirenServiceWithDeps(connectApiService.Object, connectOptions.Object);

            await sirenService.HandleSirenStatuscode(dto);

            connectApiService.Verify(s => s.PostDefectReportCategory(It.IsAny<DefectReportCategoryModel>(), It.IsAny<string>()), Times.Never);
        }

        private (SirenService, Mock<IConnectApiService>) CreateSirenService()
        {
            var log = new Mock<ILogger<SirenService>>();
            var sirenLog = new Mock<Serilog.ILogger>();
            var connectApiService = new Mock<IConnectApiService>();

            var connectOptions = new Mock<IOptions<ConnectOptions>>();
            connectOptions.Setup(o => o.Value).Returns(new ConnectOptions
            {
                Sites = [new() { Name = "Test1", Sirens = [new Siren() { Name = "Testsirene", Issi = "1234567" }], Key = "testkey" }]
            });

            var sirenStatusOptions = new Mock<IOptions<SirenStatusOptions>>();
            sirenStatusOptions.Setup(o => o.Value).Returns(new SirenStatusOptions()
            {
                FailureTranslations = new Dictionary<string, string>
                {
                    { "E001", "Nicht ausgelöst" },
                    { "E007", "Sabotagealarm, Türkontakt geöffnet" },
                    { "E008", "Fehler Netzstromversorgung" },
                    { "SPRT Sabotage: geöffnet", "Sabotagealarm, Türkontakt geöffnet" },
                }
            });

            var service = new SirenService(log.Object, connectApiService.Object, connectOptions.Object, sirenStatusOptions.Object, sirenLog.Object);
            return (service, connectApiService);
        }

        private SirenService CreateSirenServiceWithDeps(IConnectApiService connectApiService, IOptions<ConnectOptions> connectOptions)
        {
            var log = new Mock<ILogger<SirenService>>();
            var sirenLog = new Mock<Serilog.ILogger>();

            var sirenStatusOptions = new Mock<IOptions<SirenStatusOptions>>();
            sirenStatusOptions.Setup(o => o.Value).Returns(new SirenStatusOptions()
            {
                FailureTranslations = new Dictionary<string, string>
                {
                    { "E001", "Nicht ausgelöst" },
                    { "E007", "Sabotagealarm, Türkontakt geöffnet" },
                    { "E008", "Fehler Netzstromversorgung" },
                    { "SPRT Sabotage: geöffnet", "Sabotagealarm, Türkontakt geöffnet" },
                }
            });

            return new SirenService(log.Object, connectApiService, connectOptions, sirenStatusOptions.Object, sirenLog.Object);
        }
    }
}
