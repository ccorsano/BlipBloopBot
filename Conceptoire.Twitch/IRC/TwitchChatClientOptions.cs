﻿using Conceptoire.Twitch.API;

namespace Conceptoire.Twitch.IRC
{
    public class TwitchChatClientOptions
    {
        public string Endpoint { get; set; } = "wss://irc-ws.chat.twitch.tv:443";
        public string UserName { get; set; }
        public string OAuthToken { get; set; }
        public HelixValidateTokenResponse TokenInfo {get;set;}
    }
}
