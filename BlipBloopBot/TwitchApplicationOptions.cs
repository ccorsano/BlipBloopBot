using BlipBloopBot.Twitch.IRC;

namespace BlipBloopBot
{
    public class TwitchApplicationOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public TwitchChatClientOptions IrcOptions { get; set; }
    }
}
