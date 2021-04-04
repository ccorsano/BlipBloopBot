using Conceptoire.Twitch.IRC;

namespace Conceptoire.Twitch
{
    public class TwitchApplicationOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public TwitchChatClientOptions IrcOptions { get; set; }
        public string SteamApiKey { get; set; }
    }
}
