using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixUsersGetResponse
    {
        [JsonPropertyName("data")]
        public HelixUsersGetResult[] Data { get; set; }
    }
}