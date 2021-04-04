using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixEventSubSubscriptionData
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("cost")]
        public long Cost { get; set; }

        [JsonPropertyName("condition")]
        public Dictionary<string, string> Condition { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("transport")]
        public HelixEventSubTransport Transport { get; set; }
    }
}
