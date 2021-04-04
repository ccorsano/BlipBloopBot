using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixGetChannelInfoResponse
    {
        [JsonPropertyName("data")]
        public HelixChannelInfo[] Data { get; set; }
    }
}
