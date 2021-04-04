using Conceptoire.Twitch.Authentication;
using Microsoft.Extensions.Logging;
using System;

namespace Conceptoire.Twitch.IRC
{
    public interface ITwitchChatClientBuilder
    {
        public ITwitchChatClientBuilder WithLoggerFactory(ILoggerFactory loggerFactory);
        public ITwitchChatClientBuilder WithEndpoint(Uri websocketUri);
        public ITwitchChatClientBuilder WithAuthenticatedUser(IAuthenticated authenticated);
        public ITwitchChatClientBuilder WithOAuthToken(string oAuthToken);
        public ITwitchChatClient Build();
    }
}
