using FeuerSoftware.TetraControl2Connect.Hubs;
using FeuerSoftware.TetraControl2Connect.Models.TetraControl;
using FeuerSoftware.TetraControl2Connect.Services;
using FeuerSoftware.TetraControl2Connect.Shared.Options;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Xunit;

namespace FeuerSoftware.TetraControl2Connect.Test
{
    public class AgentTest
    {
        [Fact]
        public async Task HandleSDS()
        {
            var statusSubject = new Subject<TetraControlDto>();
            var positionSubject = new Subject<TetraControlDto>();
            var sdsSubject = new Subject<TetraControlDto>();
            var handleCount = 0;
            var completedHandles = new Dictionary<int, DateTime>();

            var log = new Mock<ILogger<Agent>>(MockBehavior.Loose);
            var tcClient = new Mock<ITetraControlClient>(MockBehavior.Strict);
            tcClient.Setup(c => c.Init());
            tcClient.Setup(c => c.StatusReceived).Returns(statusSubject.AsObservable());
            tcClient.Setup(c => c.PositionReceived).Returns(positionSubject.AsObservable());
            tcClient.Setup(c => c.SDSReceived).Returns(sdsSubject.AsObservable());

            var connectOptions = new Mock<IOptions<ConnectOptions>>(MockBehavior.Strict);
            connectOptions.Setup(o => o.Value).Returns(new ConnectOptions());

            var programOptions = new Mock<IOptions<ProgramOptions>>(MockBehavior.Strict);
            programOptions.Setup(o => o.Value).Returns(new ProgramOptions());

            var statusOptionsOptions = new Mock<IOptions<StatusOptions>>(MockBehavior.Strict);
            statusOptionsOptions.Setup(o => o.Value).Returns(new StatusOptions());

            var severityOptions = new Mock<IOptions<SeverityOptions>>(MockBehavior.Strict);
            severityOptions.Setup(o => o.Value).Returns(new SeverityOptions());

            var sirenCalloutOptions = new Mock<IOptions<SirenCalloutOptions>>(MockBehavior.Strict);
            sirenCalloutOptions.Setup(o => o.Value).Returns(new SirenCalloutOptions());

            var userService = new Mock<IUserService>(MockBehavior.Strict);
            userService.Setup(s => s.Initialize()).Returns(Task.CompletedTask);

            var vehicleService = new Mock<IVehicleService>(MockBehavior.Strict);
            vehicleService.Setup(s => s.Initialize()).Returns(Task.CompletedTask);

            var sdsService = new Mock<ISDSService>(MockBehavior.Strict);
            sdsService
                .Setup(s => s.HandleSds(It.IsAny<TetraControlDto>()))
                .Returns(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(4));

                    handleCount++;
                    completedHandles.Add(handleCount, DateTime.Now);
                });

            var httpClientFactory = new Mock<IHttpClientFactory>();

            var sirenService = new Mock<ISirenService>(MockBehavior.Strict);
            sirenService.Setup(s => s.Initialize()).Returns(Task.CompletedTask);
            var sirenStatusOptions = new Mock<IOptions<SirenStatusOptions>>();
            sirenStatusOptions.Setup(o => o.Value).Returns(new SirenStatusOptions());

            var sitesService = new Mock<ISitesService>(MockBehavior.Strict);
            sitesService.Setup(s => s.Initialize()).Returns(Task.CompletedTask);

            var messageHubContext = new Mock<IHubContext<MessageHub>>();
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
            messageHubContext.Setup(h => h.Clients).Returns(mockClients.Object);

            var agent = new Agent(
                log.Object,
                tcClient.Object,
                connectOptions.Object,
                programOptions.Object,
                statusOptionsOptions.Object,
                sirenStatusOptions.Object,
                userService.Object,
                vehicleService.Object,
                sdsService.Object,
                severityOptions.Object,
                sirenCalloutOptions.Object,
                httpClientFactory.Object,
                sirenService.Object,
                sitesService.Object,
                messageHubContext.Object);

            await agent.StartAsync(default);

            sdsSubject.OnNext(new TetraControlDto());
            sdsSubject.OnNext(new TetraControlDto());
            sdsSubject.OnNext(new TetraControlDto());

            await Task.Delay(TimeSpan.FromSeconds(10));

            handleCount.Should().Be(3);
            var duration = (completedHandles[3] - completedHandles[1]).Duration();
            duration.Should().BeLessThan(TimeSpan.FromSeconds(2));
        }
    }
}
