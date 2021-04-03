using BlipBloopBot.Twitch.Authentication;
using Microsoft.Extensions.Logging;
using System;

namespace BlipBloopBot.Twitch.IRC
{
    public interface ITwitchChatClientBuilder
    {
        public ITwitchChatClientBuilder WithLoggerFactory(ILoggerFactory loggerFactory);
        public ITwitchChatClientBuilder WithEndpoint(Uri websocketUri);
        public ITwitchChatClientBuilder WithAuthenticatedUser(IAuthenticated authenticated);
        public ITwitchChatClientBuilder WithOAuthToken(string oAuthToken);
        public TwitchChatClient Build();
    }
}
