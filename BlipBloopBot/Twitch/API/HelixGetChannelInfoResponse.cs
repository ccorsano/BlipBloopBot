using System.Text.Json.Serialization;

namespace BlipBloopBot.Twitch.API
{
    public class HelixGetChannelInfoResponse
    {
        [JsonPropertyName("data")]
        public HelixChannelInfo[] Data { get; set; }
    }
}
