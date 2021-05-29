using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixChannelGetInfoResponse
    {
        [JsonPropertyName("data")]
        public HelixChannelInfo[] Data { get; set; }
    }
}
