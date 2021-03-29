using BlipBloopBot.Constants;
using BlipBloopBot.Twitch;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BlipBloopBotTests
{
    public class TestTwitchFluentApi
    {
        [Fact]
        public async Task CanAuthenticateUsingFluentAPI()
        {
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
                    Content = new StringContent("{\"access_token\": \"deadbeef\",\"expires_in\": 1000,\"token_type\": \"bearer\"}"),
                })
                .Verifiable();

            var authenticated = Twitch.Authenticate()
                .FromAppCredentials("test", "test")
                .WithScope(TwitchConstants.TwitchOAuthScopes.ChatRead)
                .WithHttpMessageHandler(handlerMock.Object)
                .Build();

            var message = new HttpRequestMessage();
            await authenticated.AuthenticateMessageAsync(message);

            Assert.Equal("Bearer", message.Headers.Authorization.Scheme);
            Assert.Equal("deadbeef", message.Headers.Authorization.Parameter);
        }
    }
}
