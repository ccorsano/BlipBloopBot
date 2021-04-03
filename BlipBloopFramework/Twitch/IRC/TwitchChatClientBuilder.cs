using BlipBloopBot.Constants;
using BlipBloopBot.Twitch.Authentication;
using Microsoft.Extensions.Logging;
using System;

namespace BlipBloopBot.Twitch.IRC
{
    public class TwitchChatClientBuilder : ITwitchChatClientBuilder
    {
        private Uri _endpoint = new Uri(TwitchConstants.IRCWebSocketUri);
        private IAuthenticated _authenticated;
        private ILoggerFactory _loggerFactory;

        internal TwitchChatClientBuilder()
        {

        }

        public static ITwitchChatClientBuilder Create() => new TwitchChatClientBuilder();

        ITwitchChatClientBuilder ITwitchChatClientBuilder.WithLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            return this;
        }

        ITwitchChatClientBuilder ITwitchChatClientBuilder.WithEndpoint(Uri websocketUri)
        {
            _endpoint = websocketUri;
            return this;
        }

        ITwitchChatClientBuilder ITwitchChatClientBuilder.WithAuthenticatedUser(IAuthenticated authenticated)
        {
            _authenticated = authenticated;
            return this;
        }

        ITwitchChatClientBuilder ITwitchChatClientBuilder.WithOAuthToken(string oAuthToken)
        {
            _authenticated = new AuthenticationBuilder()
                .FromOAuthToken(oAuthToken)
                .Build();
            return this;
        }

        TwitchChatClient ITwitchChatClientBuilder.Build()
        {
            return new TwitchChatClient(_authenticated, _endpoint, _loggerFactory.CreateLogger<TwitchChatClient>());
        }
    }
}
