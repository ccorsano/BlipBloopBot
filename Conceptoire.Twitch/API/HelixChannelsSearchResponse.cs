using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixChannelsSearchResponse
    {
        [JsonPropertyName("data")]
        public HelixChannelSearchResult[] Data { get; set; }

        [JsonPropertyName("pagination")]
        public HelixResponsePagination Pagination { get; set; }
    }

    [JsonSerializable(typeof(HelixChannelsSearchResponse))]
    internal partial class HelixChannelsSearchResponseContext : JsonSerializerContext { }
}
