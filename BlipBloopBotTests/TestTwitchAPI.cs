using Conceptoire.Twitch.Constants;
using Conceptoire.Twitch;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Conceptoire.Twitch.API;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BlipBloopBotTests
{
    public class TestTwitchAPI
    {
        [Fact]
        public async Task CanDecodeStringPaginationCursor()
        {
            var authHandlerMock = new Mock<HttpMessageHandler>();
            authHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"access_token\": \"deadbeef\",\"expires_in\": 1000,\"token_type\": \"bearer\"}"),
                })
                .Verifiable();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"pagination\": \"\",\"data\": [{\"title\":null,\"broadcaster_id\":null,\"broadcaster_name\":null,\"game_id\":null,\"game_name\":null}]}"),
                })
                .Verifiable();

            var authenticated = Twitch.Authenticate()
                .FromAppCredentials("test", "test")
                .WithScope(TwitchConstants.TwitchOAuthScopes.ChatRead)
                .WithHttpMessageHandler(authHandlerMock.Object)
                .Build();

            var httpFactoryMock = new Mock<IHttpClientFactory>();
            httpFactoryMock.Setup<HttpClient>(f => f.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(handlerMock.Object));

            var message = new HttpRequestMessage();
            await authenticated.AuthenticateMessageAsync(message);

            var mockLogger = new Mock<ILogger<TwitchAPIClient>>();
            var apiClient = new TwitchAPIClient(authenticated, httpFactoryMock.Object, mockLogger.Object);

            List<HelixExtensionLiveChannel> liveChannels = new();
            await foreach(var liveChannel in apiClient.EnumerateExtensionLiveChannelsAsync("test"))
            {
                Assert.NotNull(liveChannel);
                liveChannels.Add(liveChannel);
            }
            Assert.Single(liveChannels);
        }
    }
}
