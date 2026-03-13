using Bogus;
using FeuerSoftware.TetraControl2Connect.Models.Connect;
using FeuerSoftware.TetraControl2Connect.Services;
using FeuerSoftware.TetraControl2Connect.Shared.Options;
using FeuerSoftware.TetraControl2Connect.Shared.Options.Models;
using FeuerSoftware.TetraControl2Connect.Test.Helper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FeuerSoftware.TetraControl2Connect.Test.Services
{
    public class UserServiceTest
    {
        private static readonly Faker Faker = new("de");

        [Fact]
        public async Task GetAccessTokensForUser_Positive()
        {
            var log = new Mock<ILogger<UserService>>();
            var testkey1 = Faker.Random.AlphaNumeric(500);
            var testkey2 = Faker.Random.AlphaNumeric(500);
            var options = new ConnectOptions()
            {
                Sites =
                [
                    new Site()
                    {
                        Key = testkey1,
                        Name = "TestStandort1",
                    },
                    new Site()
                    {
                        Key = testkey2,
                        Name = "TestStandort2",
                    },
                ],
            };

            var users = TestHelper.GenerateUsers(100);

            var connectOptions = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptions.Setup(o => o.CurrentValue).Returns(options);

            var connectApiService = new Mock<IConnectApiService>();
            connectApiService
                .Setup(c => c.GetUsers(It.Is<string>(k => options.Sites.Select(s => s.Key).Contains(k))))
                .ReturnsAsync(users);

            var sitesService = new Mock<ISitesService>();
            sitesService
                .Setup(s => s.GetSiteInfo(It.Is<string>(k => options.Sites.Select(s => s.Key).Contains(k))))
                .Returns(new SiteModel()
                {
                    Address = new AddressModel(),
                    Id = Faker.Random.Int(),
                    Name = Faker.Company.CompanyName(),
                    OrganizationId = Faker.Random.Int()
                });
            var service = new UserService(
                log.Object,
                connectApiService.Object,
                connectOptions.Object,
                sitesService.Object);

            await service.Initialize();

            var testUser = users.ElementAt(Faker.Random.Int(1, 99));

            var tokens = service.GetAccessTokensForUser(testUser.PagerIssi);

            tokens.Should().HaveCount(2);
            tokens.Should().Contain(testkey1);
            tokens.Should().Contain(testkey2);
        }

        [Fact]
        public async Task GetAccessTokensForUser_Negative()
        {
            var log = new Mock<ILogger<UserService>>();
            var testkey1 = Faker.Random.AlphaNumeric(500);
            var testkey2 = Faker.Random.AlphaNumeric(500);
            var options = new ConnectOptions()
            {
                Sites =
                [
                    new Site()
                    {
                        Key = testkey1,
                        Name = "TestStandort1",
                    },
                    new Site()
                    {
                        Key = testkey2,
                        Name = "TestStandort2",
                    },
                ],
            };

            var users = TestHelper.GenerateUsers(100);

            var connectOptions = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptions.Setup(o => o.CurrentValue).Returns(options);

            var connectApiService = new Mock<IConnectApiService>();
            connectApiService
                .Setup(c => c.GetUsers(It.Is<string>(k => options.Sites.Select(s => s.Key).Contains(k))))
                .ReturnsAsync(users);

            var sitesService = new Mock<ISitesService>();
            sitesService
                .Setup(s => s.GetSiteInfo(It.Is<string>(k => options.Sites.Select(s => s.Key).Contains(k))))
                .Returns(new SiteModel()
                {
                    Address = new AddressModel(),
                    Id = Faker.Random.Int(),
                    Name = Faker.Company.CompanyName(),
                    OrganizationId = Faker.Random.Int()
                });

            var service = new UserService(
                log.Object,
                connectApiService.Object,
                connectOptions.Object,
                sitesService.Object);

            await service.Initialize();

            var testUser = users.ElementAt(Faker.Random.Int(1, 99));

            var tokens = service.GetAccessTokensForUser(Guid.NewGuid().ToString());

            tokens.Should().HaveCount(0);
        }

        [Fact]
        public async Task GetUser_Positive()
        {
            var log = new Mock<ILogger<UserService>>();
            var testkey1 = Faker.Random.AlphaNumeric(500);
            var testkey2 = Faker.Random.AlphaNumeric(500);
            var options = new ConnectOptions()
            {
                Sites =
                [
                    new Site()
                    {
                        Key = testkey1,
                        Name = "TestStandort1",
                    },
                    new Site()
                    {
                        Key = testkey2,
                        Name = "TestStandort2",
                    },
                ],
            };

            var testUsers = TestHelper.GenerateUsers(100);

            var connectOptions = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptions.Setup(o => o.CurrentValue).Returns(options);

            var connectApiService = new Mock<IConnectApiService>();
            connectApiService
                .Setup(c => c.GetUsers(It.Is<string>(k => options.Sites.Select(s => s.Key).Contains(k))))
                .ReturnsAsync(testUsers);

            var sitesService = new Mock<ISitesService>();
            sitesService
                .Setup(s => s.GetSiteInfo(It.Is<string>(k => options.Sites.Select(s => s.Key).Contains(k))))
                .Returns(new SiteModel()
                {
                    Address = new AddressModel(),
                    Id = Faker.Random.Int(),
                    Name = Faker.Company.CompanyName(),
                    OrganizationId = Faker.Random.Int()
                });

            var service = new UserService(
                log.Object,
                connectApiService.Object,
                connectOptions.Object,
                sitesService.Object);

            await service.Initialize();

            var testUser = testUsers.ElementAt(Faker.Random.Int(1, 99));

            var users = service.GetUsers(testUser.PagerIssi);

            users.Should().HaveCount(1);
            users.Should().Contain(testUser);
        }

        [Fact]
        public async Task GetUser_Negative()
        {
            var log = new Mock<ILogger<UserService>>();
            var testkey1 = Faker.Random.AlphaNumeric(500);
            var testkey2 = Faker.Random.AlphaNumeric(500);
            var options = new ConnectOptions()
            {
                Sites =
                [
                    new Site()
                    {
                        Key = testkey1,
                        Name = "TestStandort1",
                    },
                    new Site()
                    {
                        Key = testkey2,
                        Name = "TestStandort2",
                    },
                ],
            };

            var testUsers = TestHelper.GenerateUsers(100);

            var connectOptions = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptions.Setup(o => o.CurrentValue).Returns(options);

            var connectApiService = new Mock<IConnectApiService>();
            connectApiService
                .Setup(c => c.GetUsers(It.Is<string>(k => options.Sites.Select(s => s.Key).Contains(k))))
                .ReturnsAsync(testUsers);

            var sitesService = new Mock<ISitesService>();
            sitesService
                .Setup(s => s.GetSiteInfo(It.Is<string>(k => options.Sites.Select(s => s.Key).Contains(k))))
                .Returns(new SiteModel()
                {
                    Address = new AddressModel(),
                    Id = Faker.Random.Int(),
                    Name = Faker.Company.CompanyName(),
                    OrganizationId = Faker.Random.Int()
                });

            var service = new UserService(
                log.Object,
                connectApiService.Object,
                connectOptions.Object,
                sitesService.Object);

            await service.Initialize();

            var testUser = testUsers.ElementAt(Faker.Random.Int(1, 99));

            var users = service.GetUsers(Guid.NewGuid().ToString());

            users.Should().BeEmpty();
        }
    }
}
