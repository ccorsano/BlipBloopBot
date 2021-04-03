using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixEventSubSubscriptionsListReponse
    {
        [JsonPropertyName("data")]
        public HelixEventSubSubscriptionData[] Data { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("total_cost")]
        public int TotalCost { get; set; }

        [JsonPropertyName("max_total_cost")]
        public int MaxTotalCost { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("pagination")]
        public HelixResponsePagination Pagination { get; set; }
    }
}
