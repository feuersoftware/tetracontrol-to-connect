using Bogus;
using FeuerSoftware.TetraControl2Connect.Models.Connect;
using FeuerSoftware.TetraControl2Connect.Models.TetraControl;
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
    public class VehicleServiceTest
    {
        private const double TestPositionVariety = 0.00005d;

        private static readonly Faker Faker = new("de");
        private readonly ProgramOptions _programOptions = new();

        [Theory]
        [InlineData(50.547634d, 9.664063d, 50.547634d + TestPositionVariety, 9.664063d + TestPositionVariety)]
        [InlineData(51.0, 9.0, 51.0 + TestPositionVariety, 9.0 + TestPositionVariety)]
        [InlineData(50.547634, 9.664063, 50.547634, 9.664063)]
        [InlineData(50.547634, 9.664063, 50.547634 + TestPositionVariety, 9.664063 - TestPositionVariety)]
        public async Task HandlePosition_WithCache_WithinTolerance(
            double firstLat, double firstLong,
            double secondLat, double secondLong)
        {
            var testkey1 = Faker.Random.AlphaNumeric(500);

            var connectOptions = new ConnectOptions()
            {
                Sites =
                [
                    new Site()
                    {
                        Key = testkey1,
                        Name = "TestStandort1",
                    },
                ],
            };

            var vehicles = TestHelper.GenerateVehicles(100);

            var issi = vehicles.First().RadioId;

            var dto1 = new TetraControlDto
            {
                Latitude = firstLat,
                Longitude = firstLong,
                SourceSSI = issi,
                TimestampUTC = DateTime.UtcNow,
            };

            var dto2 = new TetraControlDto
            {
                Latitude = secondLat,
                Longitude = secondLong,
                SourceSSI = issi,
                TimestampUTC = DateTime.UtcNow,
            };

            var log = new Mock<ILogger<VehicleService>>();

            var connectOptionsMock = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptionsMock.Setup(o => o.CurrentValue).Returns(connectOptions);

            var programOptionsMock = new Mock<IOptionsMonitor<ProgramOptions>>();
            programOptionsMock.Setup(o => o.CurrentValue).Returns(_programOptions);

            var connectApiService = new Mock<IConnectApiService>();
            connectApiService
                .Setup(c => c.GetVehicles(It.Is<string>(k => connectOptions.Sites.Select(s => s.Key).Contains(k))))
                .ReturnsAsync(vehicles);

            var numberOfPositionUpdateCalls = 0;

            connectApiService
                .Setup(c => c.PostVehiclePosition(testkey1, issi, It.IsAny<StatusPositionModel>()))
                .Callback(() => numberOfPositionUpdateCalls++);

            var service = new VehicleService(
                log.Object,
                connectApiService.Object,
                connectOptionsMock.Object,
                programOptionsMock.Object);

            await service.Initialize();

            await service.HandleVehiclePosition(dto1);

            await service.HandleVehiclePosition(dto2);

            numberOfPositionUpdateCalls.Should().Be(1);
        }

        [Theory]
        [InlineData(50.547634, 9.664063, 50.547634 + TestPositionVariety + 0.01, 9.664063 + TestPositionVariety + 0.0002)]
        [InlineData(51.0, 9.0, 51.0 - (TestPositionVariety + 0.01), 9.0 - (TestPositionVariety + 0.01))]
        [InlineData(50.547634, 9.664063, 51.547634, 10.664063)]
        public async Task HandlePosition_WithCache_NotWithinTolerance(
            double firstLat, double firstLong,
            double secondLat, double secondLong)
        {
            var testkey1 = Faker.Random.AlphaNumeric(500);

            var connectOptions = new ConnectOptions()
            {
                Sites =
                [
                    new Site()
                    {
                        Key = testkey1,
                        Name = "TestStandort1",
                    },
                ],
            };

            var vehicles = TestHelper.GenerateVehicles(100);

            var issi = vehicles.First().RadioId;

            var dto1 = new TetraControlDto
            {
                Latitude = firstLat,
                Longitude = firstLong,
                SourceSSI = issi,
                TimestampUTC = DateTime.UtcNow,
            };

            var dto2 = new TetraControlDto
            {
                Latitude = secondLat,
                Longitude = secondLong,
                SourceSSI = issi,
                TimestampUTC = DateTime.UtcNow,
            };

            var log = new Mock<ILogger<VehicleService>>();

            var connectOptionsMock = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptionsMock.Setup(o => o.CurrentValue).Returns(connectOptions);

            var programOptionsMock = new Mock<IOptionsMonitor<ProgramOptions>>();
            programOptionsMock.Setup(o => o.CurrentValue).Returns(_programOptions);

            var connectApiService = new Mock<IConnectApiService>();
            connectApiService
                .Setup(c => c.GetVehicles(It.Is<string>(k => connectOptions.Sites.Select(s => s.Key).Contains(k))))
                .ReturnsAsync(vehicles);

            var numberOfPositionUpdateCalls = 0;

            connectApiService
                .Setup(c => c.PostVehiclePosition(testkey1, issi, It.IsAny<StatusPositionModel>()))
                .Callback(() => numberOfPositionUpdateCalls++);

            var service = new VehicleService(
                log.Object,
                connectApiService.Object,
                connectOptionsMock.Object,
                programOptionsMock.Object);

            await service.Initialize();

            await service.HandleVehiclePosition(dto1);

            await service.HandleVehiclePosition(dto2);

            numberOfPositionUpdateCalls.Should().Be(2);
        }

        [Fact]
        public async Task GetAccessTokensForVehicle_Positive()
        {
            var log = new Mock<ILogger<VehicleService>>();
            var testkey1 = Faker.Random.AlphaNumeric(500);
            var testkey2 = Faker.Random.AlphaNumeric(500);
            var connectOptions = new ConnectOptions()
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

            var vehicles = TestHelper.GenerateVehicles(100);

            var connectOptionsMock = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptionsMock.Setup(o => o.CurrentValue).Returns(connectOptions);

            var programOptionsMock = new Mock<IOptionsMonitor<ProgramOptions>>();
            programOptionsMock.Setup(o => o.CurrentValue).Returns(_programOptions);

            var connectApiService = new Mock<IConnectApiService>();
            connectApiService
                .Setup(c => c.GetVehicles(It.Is<string>(k => connectOptions.Sites.Select(s => s.Key).Contains(k))))
                .ReturnsAsync(vehicles);

            var service = new VehicleService(
                log.Object,
                connectApiService.Object,
                connectOptionsMock.Object,
                programOptionsMock.Object);

            await service.Initialize();

            var testVehicle = vehicles.ElementAt(Faker.Random.Int(1, 99));

            var tokens = service.GetAccessTokensForVehicle(testVehicle.RadioId);

            tokens.Should().HaveCount(2);
            tokens.Should().Contain(testkey1);
            tokens.Should().Contain(testkey2);
        }

        [Fact]
        public async Task GetAccessTokensForVehicle_Negative()
        {
            var log = new Mock<ILogger<VehicleService>>();
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

            var vehicles = TestHelper.GenerateVehicles(100);

            var connectOptions = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptions.Setup(o => o.CurrentValue).Returns(options);

            var programOptionsMock = new Mock<IOptionsMonitor<ProgramOptions>>();
            programOptionsMock.Setup(o => o.CurrentValue).Returns(_programOptions);

            var connectApiService = new Mock<IConnectApiService>();
            connectApiService
                .Setup(c => c.GetVehicles(It.Is<string>(k => options.Sites.Select(s => s.Key).Contains(k))))
                .ReturnsAsync(vehicles);

            var service = new VehicleService(
                log.Object,
                connectApiService.Object,
                connectOptions.Object,
                programOptionsMock.Object);

            await service.Initialize();

            var tokens = service.GetAccessTokensForVehicle(Guid.NewGuid().ToString());

            tokens.Should().HaveCount(0);
        }

        [Fact]
        public async Task GetVehicle_Positive()
        {
            var log = new Mock<ILogger<VehicleService>>();
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

            var vehicles = TestHelper.GenerateVehicles(100);

            var connectOptions = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptions.Setup(o => o.CurrentValue).Returns(options);

            var programOptionsMock = new Mock<IOptionsMonitor<ProgramOptions>>();
            programOptionsMock.Setup(o => o.CurrentValue).Returns(_programOptions);

            var connectApiService = new Mock<IConnectApiService>();
            connectApiService
                .Setup(c => c.GetVehicles(It.Is<string>(k => options.Sites.Select(s => s.Key).Contains(k))))
                .ReturnsAsync(vehicles);

            var service = new VehicleService(
                log.Object,
                connectApiService.Object,
                connectOptions.Object,
                programOptionsMock.Object);

            await service.Initialize();

            var testVehicle = vehicles.ElementAt(Faker.Random.Int(1, 99));

            var vehicle = service.GetVehicle(testVehicle.RadioId);

            vehicle.Should().NotBeNull();
            vehicle.Should().Be(testVehicle);
        }

        [Fact]
        public async Task GetVehicle_Negative()
        {
            var log = new Mock<ILogger<VehicleService>>();
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

            var vehicles = TestHelper.GenerateVehicles(100);

            var connectOptions = new Mock<IOptionsMonitor<ConnectOptions>>();
            connectOptions.Setup(o => o.CurrentValue).Returns(options);

            var programOptionsMock = new Mock<IOptionsMonitor<ProgramOptions>>();
            programOptionsMock.Setup(o => o.CurrentValue).Returns(_programOptions);

            var connectApiService = new Mock<IConnectApiService>();
            connectApiService
                .Setup(c => c.GetVehicles(It.Is<string>(k => options.Sites.Select(s => s.Key).Contains(k))))
                .ReturnsAsync(vehicles);

            var service = new VehicleService(
                log.Object,
                connectApiService.Object,
                connectOptions.Object,
                programOptionsMock.Object);

            await service.Initialize();

            var vehicle = service.GetVehicle(Guid.NewGuid().ToString());

            vehicle.Should().BeNull();
        }
    }
}
