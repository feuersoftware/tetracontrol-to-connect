using FeuerSoftware.TetraControl2Connect.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FeuerSoftware.TetraControl2Connect.Test.Services
{
    public class UpdateServiceTest
    {
        private readonly Mock<ILogger<UpdateService>> _log = new();
        private readonly Mock<IHttpClientFactory> _httpClientFactory = new();

        private UpdateService CreateService(MockHttpMessageHandler handler)
        {
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.github.com/")
            };

            _httpClientFactory
                .Setup(f => f.CreateClient(nameof(IUpdateService)))
                .Returns(client);

            return new UpdateService(_log.Object, _httpClientFactory.Object);
        }

        [Fact]
        public async Task CheckForUpdateAsync_NewVersionAvailable_ReturnsUpdateInfo()
        {
            var json = """
                {
                    "tag_name": "v99.0.0",
                    "html_url": "https://github.com/feuersoftware/tetracontrol-to-connect/releases/tag/v99.0.0",
                    "prerelease": false,
                    "draft": false
                }
                """;
            var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.CheckForUpdateAsync();

            result.Should().NotBeNull();
            result!.LatestVersion.Should().Be("v99.0.0");
            result.ReleaseUrl.Should().Be("https://github.com/feuersoftware/tetracontrol-to-connect/releases/tag/v99.0.0");
            service.LatestUpdate.Should().Be(result);
        }

        [Fact]
        public async Task CheckForUpdateAsync_SameVersion_ReturnsNull()
        {
            // Use version 0.0.0 which is always <= any real current version
            var currentVersion = typeof(Agent).Assembly.GetName().Version;
            var currentTag = $"v{currentVersion!.Major}.{currentVersion.Minor}.{currentVersion.Build}";

            var json = $$"""
                {
                    "tag_name": "{{currentTag}}",
                    "html_url": "https://github.com/feuersoftware/tetracontrol-to-connect/releases/tag/{{currentTag}}",
                    "prerelease": false,
                    "draft": false
                }
                """;
            var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.CheckForUpdateAsync();

            result.Should().BeNull();
            service.LatestUpdate.Should().BeNull();
        }

        [Fact]
        public async Task CheckForUpdateAsync_Prerelease_ReturnsNull()
        {
            var json = """
                {
                    "tag_name": "v99.0.0",
                    "html_url": "https://github.com/feuersoftware/tetracontrol-to-connect/releases/tag/v99.0.0",
                    "prerelease": true,
                    "draft": false
                }
                """;
            var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.CheckForUpdateAsync();

            result.Should().BeNull();
            service.LatestUpdate.Should().BeNull();
        }

        [Fact]
        public async Task CheckForUpdateAsync_Draft_ReturnsNull()
        {
            var json = """
                {
                    "tag_name": "v99.0.0",
                    "html_url": "https://github.com/feuersoftware/tetracontrol-to-connect/releases/tag/v99.0.0",
                    "prerelease": false,
                    "draft": true
                }
                """;
            var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.CheckForUpdateAsync();

            result.Should().BeNull();
            service.LatestUpdate.Should().BeNull();
        }

        [Fact]
        public async Task CheckForUpdateAsync_HttpError_ReturnsNull()
        {
            var handler = new MockHttpMessageHandler(string.Empty, HttpStatusCode.NotFound);
            var service = CreateService(handler);

            var result = await service.CheckForUpdateAsync();

            result.Should().BeNull();
            service.LatestUpdate.Should().BeNull();
        }

        [Fact]
        public async Task CheckForUpdateAsync_NetworkException_ReturnsNull()
        {
            var handler = new MockHttpMessageHandler(new HttpRequestException("Network error"));
            var service = CreateService(handler);

            var result = await service.CheckForUpdateAsync();

            result.Should().BeNull();
            service.LatestUpdate.Should().BeNull();
        }

        [Fact]
        public async Task CheckForUpdateAsync_InvalidTagFormat_ReturnsNull()
        {
            var json = """
                {
                    "tag_name": "not-a-version",
                    "html_url": "https://github.com/feuersoftware/tetracontrol-to-connect/releases/tag/not-a-version",
                    "prerelease": false,
                    "draft": false
                }
                """;
            var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.CheckForUpdateAsync();

            result.Should().BeNull();
            service.LatestUpdate.Should().BeNull();
        }

        [Fact]
        public async Task CheckForUpdateAsync_OlderVersion_ReturnsNull()
        {
            var json = """
                {
                    "tag_name": "v0.0.1",
                    "html_url": "https://github.com/feuersoftware/tetracontrol-to-connect/releases/tag/v0.0.1",
                    "prerelease": false,
                    "draft": false
                }
                """;
            var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
            var service = CreateService(handler);

            var result = await service.CheckForUpdateAsync();

            result.Should().BeNull();
            service.LatestUpdate.Should().BeNull();
        }
    }
}
