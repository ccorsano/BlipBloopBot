using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixChannelGetModeratorsResponse
    {
        [JsonPropertyName("data")]
        public HelixChannelModerator[] Data { get; set; }

        [JsonPropertyName("pagination")]
        public HelixResponsePagination Pagination { get; set; }
    }
}