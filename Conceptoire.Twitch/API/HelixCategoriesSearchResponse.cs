using Conceptoire.Twitch.API;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixCategoriesSearchResponse
    {
        [JsonPropertyName("data")]
        public HelixCategoriesSearchEntry[] Data { get; set; }

        [JsonPropertyName("pagination")]
        public HelixResponsePagination Pagination { get; set; }
    }
}
