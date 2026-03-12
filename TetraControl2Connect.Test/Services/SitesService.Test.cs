using Bogus;
using FeuerSoftware.TetraControl2Connect.Models.Connect;
using FeuerSoftware.TetraControl2Connect.Services;
using FeuerSoftware.TetraControl2Connect.Shared.Options;
using FeuerSoftware.TetraControl2Connect.Shared.Options.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FeuerSoftware.TetraControl2Connect.Test.Services
{
    public class SitesServiceTest
    {
        private static readonly Faker Faker = new("de");

        private static SiteModel GenerateSiteModel(int? organizationId = null)
        {
            return new SiteModel
            {
                Id = Faker.Random.Int(1, 999999),
                Name = Faker.Company.CompanyName(),
                Address = new AddressModel
                {
                    Street = Faker.Address.StreetName(),
                    HouseNumber = Faker.Address.BuildingNumber(),
                    ZipCode = Faker.Address.ZipCode(),
                    City = Faker.Address.City(),
                },
                OrganizationId = organizationId ?? 0,
            };
        }

        private static OrganizationModel GenerateOrganizationModel(int siteCount = 1)
        {
            var org = new OrganizationModel
            {
                Id = Faker.Random.Int(1, 999999),
                Name = Faker.Company.CompanyName(),
                Sites = [],
            };

            for (var i = 0; i < siteCount; i++)
                org.Sites.Add(GenerateSiteModel());

            return org;
        }

        private static (SitesService Service, Mock<IConnectApiService> ApiMock, Mock<ILogger<SitesService>> LogMock)
            CreateService(ConnectOptions connectOptions)
        {
            var log = new Mock<ILogger<SitesService>>();
            var connectApiService = new Mock<IConnectApiService>();

            var connectOptionsMock = new Mock<IOptions<ConnectOptions>>();
            connectOptionsMock.Setup(o => o.Value).Returns(connectOptions);

            var service = new SitesService(
                log.Object,
                connectApiService.Object,
                connectOptionsMock.Object);

            return (service, connectApiService, log);
        }

        [Fact]
        public void GetSiteInfo_NotInitialized_ThrowsInvalidOperationException()
        {
            var connectOptions = new ConnectOptions();
            var (service, _, _) = CreateService(connectOptions);

            var act = () => service.GetSiteInfo(Faker.Random.AlphaNumeric(500));

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*not initialized*");
        }

        [Fact]
        public async Task GetSiteInfo_AfterInitialization_ReturnsCorrectSiteModel()
        {
            var testKey = Faker.Random.AlphaNumeric(500);
            var connectOptions = new ConnectOptions
            {
                Sites = [new Site { Key = testKey, Name = "TestStandort1" }],
            };

            var organization = GenerateOrganizationModel(1);
            var expectedSite = organization.Sites[0];

            var (service, apiMock, _) = CreateService(connectOptions);
            apiMock.Setup(c => c.GetOrganizationInfo(testKey)).ReturnsAsync(organization);

            await service.Initialize();

            var result = service.GetSiteInfo(testKey);

            result.Should().BeSameAs(expectedSite);
            result.Name.Should().Be(expectedSite.Name);
            result.Id.Should().Be(expectedSite.Id);
        }

        [Fact]
        public async Task GetSiteInfo_UnknownAccessToken_ThrowsKeyNotFoundException()
        {
            var testKey = Faker.Random.AlphaNumeric(500);
            var connectOptions = new ConnectOptions
            {
                Sites = [new Site { Key = testKey, Name = "TestStandort1" }],
            };

            var organization = GenerateOrganizationModel(1);

            var (service, apiMock, _) = CreateService(connectOptions);
            apiMock.Setup(c => c.GetOrganizationInfo(testKey)).ReturnsAsync(organization);

            await service.Initialize();

            var act = () => service.GetSiteInfo("unknown-key");

            act.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public async Task Initialize_SingleSite_AddsCorrectly()
        {
            var testKey = Faker.Random.AlphaNumeric(500);
            var connectOptions = new ConnectOptions
            {
                Sites = [new Site { Key = testKey, Name = "TestStandort1" }],
            };

            var organization = GenerateOrganizationModel(1);

            var (service, apiMock, _) = CreateService(connectOptions);
            apiMock.Setup(c => c.GetOrganizationInfo(testKey)).ReturnsAsync(organization);

            await service.Initialize();

            var result = service.GetSiteInfo(testKey);
            result.Should().NotBeNull();
            result.Id.Should().Be(organization.Sites[0].Id);
        }

        [Fact]
        public async Task Initialize_MultipleSites_AllAdded()
        {
            var testKey1 = Faker.Random.AlphaNumeric(500);
            var testKey2 = Faker.Random.AlphaNumeric(500);
            var connectOptions = new ConnectOptions
            {
                Sites =
                [
                    new Site { Key = testKey1, Name = "TestStandort1" },
                    new Site { Key = testKey2, Name = "TestStandort2" },
                ],
            };

            var org1 = GenerateOrganizationModel(1);
            var org2 = GenerateOrganizationModel(1);

            var (service, apiMock, _) = CreateService(connectOptions);
            apiMock.Setup(c => c.GetOrganizationInfo(testKey1)).ReturnsAsync(org1);
            apiMock.Setup(c => c.GetOrganizationInfo(testKey2)).ReturnsAsync(org2);

            await service.Initialize();

            var site1 = service.GetSiteInfo(testKey1);
            var site2 = service.GetSiteInfo(testKey2);

            site1.Should().BeSameAs(org1.Sites[0]);
            site2.Should().BeSameAs(org2.Sites[0]);
        }

        [Fact]
        public async Task Initialize_GetOrganizationInfoReturnsNull_LogsCriticalAndContinues()
        {
            var testKey1 = Faker.Random.AlphaNumeric(500);
            var testKey2 = Faker.Random.AlphaNumeric(500);
            var connectOptions = new ConnectOptions
            {
                Sites =
                [
                    new Site { Key = testKey1, Name = "NullStandort" },
                    new Site { Key = testKey2, Name = "TestStandort2" },
                ],
            };

            var org2 = GenerateOrganizationModel(1);

            var (service, apiMock, logMock) = CreateService(connectOptions);
            apiMock.Setup(c => c.GetOrganizationInfo(testKey1)).ReturnsAsync((OrganizationModel?)null);
            apiMock.Setup(c => c.GetOrganizationInfo(testKey2)).ReturnsAsync(org2);

            await service.Initialize();

            // Second site should still be accessible
            var site2 = service.GetSiteInfo(testKey2);
            site2.Should().NotBeNull();

            // First site should not be present
            var act = () => service.GetSiteInfo(testKey1);
            act.Should().Throw<KeyNotFoundException>();

            logMock.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("NullStandort")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Initialize_OrganizationHasMultipleSites_LogsErrorAndContinues()
        {
            var testKey1 = Faker.Random.AlphaNumeric(500);
            var testKey2 = Faker.Random.AlphaNumeric(500);
            var connectOptions = new ConnectOptions
            {
                Sites =
                [
                    new Site { Key = testKey1, Name = "OrgStandort" },
                    new Site { Key = testKey2, Name = "TestStandort2" },
                ],
            };

            var orgWithMultipleSites = GenerateOrganizationModel(3);
            var org2 = GenerateOrganizationModel(1);

            var (service, apiMock, logMock) = CreateService(connectOptions);
            apiMock.Setup(c => c.GetOrganizationInfo(testKey1)).ReturnsAsync(orgWithMultipleSites);
            apiMock.Setup(c => c.GetOrganizationInfo(testKey2)).ReturnsAsync(org2);

            await service.Initialize();

            // The org-key site throws InvalidDataException which is caught, so it shouldn't be added
            var act = () => service.GetSiteInfo(testKey1);
            act.Should().Throw<KeyNotFoundException>();

            // Second site should still work
            var site2 = service.GetSiteInfo(testKey2);
            site2.Should().NotBeNull();

            logMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("OrgStandort")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Initialize_GetOrganizationInfoThrowsException_LogsErrorAndContinues()
        {
            var testKey1 = Faker.Random.AlphaNumeric(500);
            var testKey2 = Faker.Random.AlphaNumeric(500);
            var connectOptions = new ConnectOptions
            {
                Sites =
                [
                    new Site { Key = testKey1, Name = "ErrorStandort" },
                    new Site { Key = testKey2, Name = "TestStandort2" },
                ],
            };

            var org2 = GenerateOrganizationModel(1);

            var (service, apiMock, logMock) = CreateService(connectOptions);
            apiMock.Setup(c => c.GetOrganizationInfo(testKey1)).ThrowsAsync(new HttpRequestException("Connection failed"));
            apiMock.Setup(c => c.GetOrganizationInfo(testKey2)).ReturnsAsync(org2);

            await service.Initialize();

            // First site should not be present
            var act = () => service.GetSiteInfo(testKey1);
            act.Should().Throw<KeyNotFoundException>();

            // Second site should still work
            var site2 = service.GetSiteInfo(testKey2);
            site2.Should().NotBeNull();

            logMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ErrorStandort")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Initialize_SetsOrganizationIdOnSiteModel()
        {
            var testKey = Faker.Random.AlphaNumeric(500);
            var connectOptions = new ConnectOptions
            {
                Sites = [new Site { Key = testKey, Name = "TestStandort1" }],
            };

            var organization = GenerateOrganizationModel(1);

            var (service, apiMock, _) = CreateService(connectOptions);
            apiMock.Setup(c => c.GetOrganizationInfo(testKey)).ReturnsAsync(organization);

            await service.Initialize();

            var result = service.GetSiteInfo(testKey);
            result.OrganizationId.Should().Be(organization.Id);
        }

        [Fact]
        public async Task Initialize_DuplicateKeys_LogsWarning()
        {
            var duplicateKey = Faker.Random.AlphaNumeric(500);
            var connectOptions = new ConnectOptions
            {
                Sites =
                [
                    new Site { Key = duplicateKey, Name = "Standort1" },
                    new Site { Key = duplicateKey, Name = "Standort2" },
                ],
            };

            var org1 = GenerateOrganizationModel(1);
            var org2 = GenerateOrganizationModel(1);

            var (service, apiMock, logMock) = CreateService(connectOptions);
            apiMock.Setup(c => c.GetOrganizationInfo(duplicateKey))
                .ReturnsAsync(org1);

            await service.Initialize();

            logMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Standort2")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
