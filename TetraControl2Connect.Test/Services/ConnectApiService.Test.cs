using Bogus;
using FeuerSoftware.TetraControl2Connect.Models.Connect;
using FeuerSoftware.TetraControl2Connect.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FeuerSoftware.TetraControl2Connect.Test.Services
{
    public class ConnectApiServiceTest
    {
        private static readonly Faker Faker = new("de");

        private readonly Mock<ILogger<ConnectApiService>> _log = new();
        private readonly Mock<IHttpClientFactory> _httpClientFactory = new();

        private ConnectApiService CreateService(MockHttpMessageHandler handler)
        {
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://connect.test.local")
            };

            _httpClientFactory
                .Setup(f => f.CreateClient(nameof(IConnectApiService)))
                .Returns(client);

            return new ConnectApiService(_log.Object, _httpClientFactory.Object);
        }

        private static string GenerateAccessToken() => Faker.Random.AlphaNumeric(500);

        #region GET - Successful Requests

        [Fact]
        public async Task GetOrganizationInfo_Success_ReturnsDeserializedModel()
        {
            var expected = new OrganizationModel
            {
                Id = Faker.Random.Int(1, 9999),
                Name = Faker.Company.CompanyName(),
                Sites =
                [
                    new SiteModel
                    {
                        Id = Faker.Random.Int(1, 999),
                        OrganizationId = Faker.Random.Int(1, 999),
                        Name = Faker.Address.City(),
                        Address = new AddressModel
                        {
                            Street = Faker.Address.StreetName(),
                            City = Faker.Address.City()
                        }
                    }
                ]
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler(JsonSerializer.Serialize(expected), HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.GetOrganizationInfo(token);

            result.Should().NotBeNull();
            result!.Id.Should().Be(expected.Id);
            result.Name.Should().Be(expected.Name);
            result.Sites.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetUsers_Success_ReturnsDeserializedList()
        {
            var expected = new List<UserModel>
            {
                new()
                {
                    Id = Faker.Random.Guid().ToString(),
                    FirstName = Faker.Name.FirstName(),
                    LastName = Faker.Name.LastName(),
                    Email = Faker.Internet.Email(),
                    UserName = Faker.Internet.UserName(),
                    PagerIssi = Faker.Random.AlphaNumeric(7)
                },
                new()
                {
                    Id = Faker.Random.Guid().ToString(),
                    FirstName = Faker.Name.FirstName(),
                    LastName = Faker.Name.LastName(),
                    Email = Faker.Internet.Email(),
                    UserName = Faker.Internet.UserName(),
                    PagerIssi = Faker.Random.AlphaNumeric(7)
                }
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler(JsonSerializer.Serialize(expected), HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.GetUsers(token);

            result.Should().HaveCount(2);
            handler.LastRequestUri.Should().Contain("/interfaces/public/user");
        }

        [Fact]
        public async Task GetVehicles_Success_ReturnsDeserializedList()
        {
            var expected = new List<VehicleModel>
            {
                new()
                {
                    Id = Faker.Random.Int(1, 9999),
                    RadioId = Faker.Random.Guid().ToString(),
                    PlaceName = Faker.Address.City(),
                    OrganizationCallSign = Faker.Random.AlphaNumeric(10),
                    CallSign = Faker.Random.AlphaNumeric(8)
                }
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler(JsonSerializer.Serialize(expected), HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.GetVehicles(token);

            result.Should().HaveCount(1);
            handler.LastRequestUri.Should().Contain("/interfaces/public/vehicle");
        }

        [Fact]
        public async Task GetDefectReports_Success_ReturnsDeserializedList()
        {
            var expected = new List<DefectReportModel>
            {
                new()
                {
                    Id = Faker.Random.Int(1, 999),
                    SiteId = Faker.Random.Int(1, 99),
                    Status = DefectReportStatus.Reported,
                    ShortDescription = Faker.Lorem.Sentence(),
                    DetailedDescription = Faker.Lorem.Paragraph(),
                    Priority = Priority.High,
                    SequenceNumber = Faker.Random.AlphaNumeric(10),
                    CategoryId = Faker.Random.Int(1, 50),
                    CreatedAt = DateTime.UtcNow
                }
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler(JsonSerializer.Serialize(expected), HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.GetDefectReports(token);

            result.Should().HaveCount(1);
            result.First().ShortDescription.Should().Be(expected.First().ShortDescription);
            handler.LastRequestUri.Should().Contain("/interfaces/public/defectReport");
        }

        [Fact]
        public async Task GetDefectReportCategories_Success_ReturnsDeserializedList()
        {
            var expected = new List<DefectReportCategoryModel>
            {
                new()
                {
                    Id = Faker.Random.Int(1, 999),
                    Name = Faker.Commerce.Categories(1).First(),
                    OrganizationId = Faker.Random.Int(1, 99),
                    SiteId = Faker.Random.Int(1, 99),
                    IsBuiltin = false
                }
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler(JsonSerializer.Serialize(expected), HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.GetDefectReportCategories(token);

            result.Should().HaveCount(1);
            result.First().Name.Should().Be(expected.First().Name);
            handler.LastRequestUri.Should().Contain("/interfaces/public/defectReportCategory");
        }

        [Fact]
        public async Task GetLatestOperations_Success_ReturnsDeserializedList()
        {
            var expected = new List<OperationModel>
            {
                new()
                {
                    Start = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Keyword = Faker.Random.AlphaNumeric(10),
                    Number = Faker.Random.AlphaNumeric(8)
                }
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler(JsonSerializer.Serialize(expected), HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.GetLatestOperations(token);

            result.Should().HaveCount(1);
            handler.LastRequestUri.Should().Contain("/interfaces/public/operation");
        }

        #endregion

        #region POST - Successful Requests

        [Fact]
        public async Task PostDefectReport_Success_SendsPostRequest()
        {
            var model = new DefectReportModel
            {
                SiteId = Faker.Random.Int(1, 99),
                Status = DefectReportStatus.Reported,
                ShortDescription = Faker.Lorem.Sentence(),
                DetailedDescription = Faker.Lorem.Paragraph(),
                Priority = Priority.Medium
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.OK);
            var service = CreateService(handler);

            await service.PostDefectReport(model, token);

            handler.LastRequestMethod.Should().Be(HttpMethod.Post);
            handler.LastRequestUri.Should().Contain("/interfaces/public/defectReport");
            handler.LastRequestBody.Should().Contain(model.ShortDescription);
        }

        [Fact]
        public async Task PostVehicleStatus_Success_SendsPostRequest()
        {
            var issi = Faker.Random.Guid().ToString();
            var model = new StatusModel
            {
                Status = Faker.Random.Byte(),
                StatusTimestamp = DateTimeOffset.UtcNow,
                Source = "TetraControl"
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.OK);
            var service = CreateService(handler);

            await service.PostVehicleStatus(token, issi, model);

            handler.LastRequestMethod.Should().Be(HttpMethod.Post);
            handler.LastRequestUri.Should().Contain($"/interfaces/public/vehicle/{issi}/status");
        }

        [Fact]
        public async Task PostVehiclePosition_Success_SendsPostRequest()
        {
            var issi = Faker.Random.Guid().ToString();
            var model = new StatusPositionModel
            {
                Position = new PositionModel
                {
                    Latitude = Faker.Address.Latitude(),
                    Longitude = Faker.Address.Longitude()
                },
                PositionTimestamp = DateTimeOffset.UtcNow,
                Source = "TetraControl"
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.OK);
            var service = CreateService(handler);

            await service.PostVehiclePosition(token, issi, model);

            handler.LastRequestMethod.Should().Be(HttpMethod.Post);
            handler.LastRequestUri.Should().Contain($"/interfaces/public/vehicle/{issi}/status");
        }

        [Fact]
        public async Task PostUserStatus_Success_SendsPostRequest()
        {
            var model = new UserStatusModel
            {
                PagerIssi = Faker.Random.AlphaNumeric(7),
                Status = UserOperationStatus.Coming
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.OK);
            var service = CreateService(handler);

            await service.PostUserStatus(token, model);

            handler.LastRequestMethod.Should().Be(HttpMethod.Post);
            handler.LastRequestUri.Should().Contain("/interfaces/public/operation/userstatus");
        }

        [Theory]
        [InlineData(UpdateStrategy.ByNumber)]
        [InlineData(UpdateStrategy.ByAddress)]
        [InlineData(UpdateStrategy.ByPosition)]
        [InlineData(UpdateStrategy.None)]
        public async Task PostOperation_Success_SendsPostRequestWithUpdateStrategy(UpdateStrategy strategy)
        {
            var model = new OperationModel
            {
                Start = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Keyword = Faker.Random.AlphaNumeric(10),
                Number = Faker.Random.AlphaNumeric(8)
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.OK);
            var service = CreateService(handler);

            await service.PostOperation(token, model, strategy);

            handler.LastRequestMethod.Should().Be(HttpMethod.Post);
            handler.LastRequestUri.Should().Contain($"/interfaces/public/operation?updateStrategy={strategy}");
        }

        [Fact]
        public async Task PostDefectReportCategory_Success_SendsPostRequest()
        {
            var model = new DefectReportCategoryModel
            {
                Name = Faker.Commerce.Categories(1).First(),
                OrganizationId = Faker.Random.Int(1, 99),
                SiteId = Faker.Random.Int(1, 99),
                IsBuiltin = false
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.OK);
            var service = CreateService(handler);

            await service.PostDefectReportCategory(model, token);

            handler.LastRequestMethod.Should().Be(HttpMethod.Post);
            handler.LastRequestUri.Should().Contain("/interfaces/public/defectReportCategory");
        }

        #endregion

        #region PUT - Successful Requests

        [Fact]
        public async Task PutDefectReport_Success_SendsPutRequest()
        {
            var id = Faker.Random.Int(1, 999);
            var model = new DefectReportModel
            {
                Id = id,
                SiteId = Faker.Random.Int(1, 99),
                Status = DefectReportStatus.InProcess,
                ShortDescription = Faker.Lorem.Sentence()
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.OK);
            var service = CreateService(handler);

            await service.PutDefectReport(id, model, token);

            handler.LastRequestMethod.Should().Be(HttpMethod.Put);
            handler.LastRequestUri.Should().Contain($"/interfaces/public/defectReport/{id}");
        }

        [Fact]
        public async Task PutUserAvailability_Success_SendsPutRequest()
        {
            var userIssi = Faker.Random.AlphaNumeric(7);
            var model = new UserAvailabilityModel
            {
                Status = AvailabilityStatus.Available,
                Source = AvailabilitySource.Pager,
                Until = DateTime.UtcNow.AddHours(8)
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.OK);
            var service = CreateService(handler);

            await service.PutUserAvailability(token, userIssi, model);

            handler.LastRequestMethod.Should().Be(HttpMethod.Put);
            handler.LastRequestUri.Should().Contain($"/interfaces/public/user/{userIssi}/availability/current");
        }

        #endregion

        #region Error Handling - Non-Success Status Code

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task GetOrganizationInfo_NonSuccessStatusCode_ReturnsDefaultAndLogsError(HttpStatusCode statusCode)
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", statusCode);
            var service = CreateService(handler);

            var result = await service.GetOrganizationInfo(token);

            result.Should().BeNull();
            _log.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unsuccessful HTTP-Request")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetUsers_NonSuccessStatusCode_ReturnsEmptyCollection()
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.InternalServerError);
            var service = CreateService(handler);

            var result = await service.GetUsers(token);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetVehicles_NonSuccessStatusCode_ReturnsEmptyCollection()
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.InternalServerError);
            var service = CreateService(handler);

            var result = await service.GetVehicles(token);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDefectReports_NonSuccessStatusCode_ReturnsEmptyList()
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.InternalServerError);
            var service = CreateService(handler);

            var result = await service.GetDefectReports(token);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetLatestOperations_NonSuccessStatusCode_ReturnsEmptyCollection()
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.InternalServerError);
            var service = CreateService(handler);

            var result = await service.GetLatestOperations(token);

            result.Should().BeEmpty();
        }

        #endregion

        #region Error Handling - Exception During HTTP Call

        [Fact]
        public async Task GetOrganizationInfo_ExceptionThrown_ReturnsDefaultAndLogsError()
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler(new HttpRequestException("Connection refused"));
            var service = CreateService(handler);

            var result = await service.GetOrganizationInfo(token);

            result.Should().BeNull();
            _log.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error happened on CallService")),
                    It.IsAny<HttpRequestException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetUsers_ExceptionThrown_ReturnsEmptyCollection()
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler(new HttpRequestException("Timeout"));
            var service = CreateService(handler);

            var result = await service.GetUsers(token);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task PostOperation_ExceptionThrown_DoesNotThrow()
        {
            var model = new OperationModel
            {
                Start = DateTime.UtcNow,
                Keyword = Faker.Random.AlphaNumeric(10)
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler(new HttpRequestException("Network error"));
            var service = CreateService(handler);

            var act = () => service.PostOperation(token, model);

            await act.Should().NotThrowAsync();
        }

        #endregion

        #region Empty Response Content

        [Fact]
        public async Task GetOrganizationInfo_EmptyResponseContent_ReturnsDefault()
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.GetOrganizationInfo(token);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUsers_EmptyResponseContent_ReturnsEmptyCollection()
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.GetUsers(token);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDefectReports_EmptyResponseContent_ReturnsEmptyList()
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.GetDefectReports(token);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDefectReportCategories_EmptyResponseContent_ReturnsEmptyList()
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("", HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.GetDefectReportCategories(token);

            result.Should().BeEmpty();
        }

        #endregion

        #region GetLatestOperations - Ordering

        [Fact]
        public async Task GetLatestOperations_OrdersByCreatedAtDescThenLastUpdateAtAsc()
        {
            var operations = new List<OperationModel>
            {
                new()
                {
                    CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                    LastUpdateAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                    Keyword = "Alpha",
                    Number = "1"
                },
                new()
                {
                    CreatedAt = new DateTime(2024, 1, 3, 10, 0, 0, DateTimeKind.Utc),
                    LastUpdateAt = new DateTime(2024, 1, 3, 14, 0, 0, DateTimeKind.Utc),
                    Keyword = "Charlie",
                    Number = "3"
                },
                new()
                {
                    CreatedAt = new DateTime(2024, 1, 3, 10, 0, 0, DateTimeKind.Utc),
                    LastUpdateAt = new DateTime(2024, 1, 3, 11, 0, 0, DateTimeKind.Utc),
                    Keyword = "Bravo",
                    Number = "2"
                },
                new()
                {
                    CreatedAt = new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                    LastUpdateAt = null,
                    Keyword = "Delta",
                    Number = "4"
                }
            };
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler(JsonSerializer.Serialize(operations), HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = (await service.GetLatestOperations(token)).ToList();

            result.Should().HaveCount(4);
            // CreatedAt desc: Jan 3 (Bravo/Charlie), Jan 2 (Delta), Jan 1 (Alpha)
            // For same CreatedAt, ThenBy LastUpdateAt asc: Bravo (11:00) before Charlie (14:00)
            result[0].Keyword.Should().Be("Bravo");
            result[1].Keyword.Should().Be("Charlie");
            result[2].Keyword.Should().Be("Delta");
            result[3].Keyword.Should().Be("Alpha");
        }

        [Fact]
        public async Task GetLatestOperations_NullResponse_ReturnsEmpty()
        {
            var token = GenerateAccessToken();
            // Empty content with 200 results in null deserialization for reference types
            var handler = new MockHttpMessageHandler("", HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.GetLatestOperations(token);

            result.Should().BeEmpty();
        }

        #endregion

        #region GetDefectReports - Null Response

        [Fact]
        public async Task GetDefectReports_NullResponse_ReturnsEmptyList()
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("null", HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.GetDefectReports(token);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDefectReportCategories_NullResponse_ReturnsEmptyList()
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler("null", HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.GetDefectReportCategories(token);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region Bearer Token Authorization Header

        [Fact]
        public async Task CallService_SetsBearerTokenInAuthorizationHeader()
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler(JsonSerializer.Serialize(new List<UserModel>()), HttpStatusCode.OK);
            var service = CreateService(handler);

            await service.GetUsers(token);

            handler.LastAuthorizationHeader.Should().NotBeNull();
            handler.LastAuthorizationHeader!.Scheme.Should().Be("Bearer");
            handler.LastAuthorizationHeader.Parameter.Should().Be(token);
        }

        [Fact]
        public async Task CallService_UsesNamedHttpClient()
        {
            var token = GenerateAccessToken();
            var handler = new MockHttpMessageHandler(JsonSerializer.Serialize(new List<VehicleModel>()), HttpStatusCode.OK);
            var service = CreateService(handler);

            await service.GetVehicles(token);

            _httpClientFactory.Verify(f => f.CreateClient(nameof(IConnectApiService)), Times.Once);
        }

        #endregion
    }

    /// <summary>
    /// Mock HttpMessageHandler to intercept and inspect HTTP requests made by HttpClient.
    /// </summary>
    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string? _responseContent;
        private readonly HttpStatusCode _statusCode;
        private readonly Exception? _exceptionToThrow;

        public HttpMethod? LastRequestMethod { get; private set; }
        public string? LastRequestUri { get; private set; }
        public string? LastRequestBody { get; private set; }
        public AuthenticationHeaderValue? LastAuthorizationHeader { get; private set; }

        public MockHttpMessageHandler(string responseContent, HttpStatusCode statusCode)
        {
            _responseContent = responseContent;
            _statusCode = statusCode;
        }

        public MockHttpMessageHandler(Exception exceptionToThrow)
        {
            _exceptionToThrow = exceptionToThrow;
            _statusCode = HttpStatusCode.OK;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequestMethod = request.Method;
            LastRequestUri = request.RequestUri?.ToString();
            LastAuthorizationHeader = request.Headers.Authorization;

            if (request.Content is not null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            if (_exceptionToThrow is not null)
            {
                throw _exceptionToThrow;
            }

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent ?? string.Empty),
                RequestMessage = request
            };
        }
    }
}
