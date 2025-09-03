using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixGetStreamsEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }

        [JsonPropertyName("game_id")]
        public string GameId { get; set; }

        [JsonPropertyName("game_name")]
        public string GameName { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("tags")]
        public string[] Tags { get; set; }

        [JsonPropertyName("viewer_count")]
        public uint ViewerCount { get; set; }

        [JsonPropertyName("started_at")]
        public DateTime StartedAt { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonIgnore]
        public Uri ThumbnailUri => string.IsNullOrEmpty(ThumbnailUrl) ? null : new Uri(ThumbnailUrl);

        [JsonPropertyName("is_mature")]
        public bool IsMature { get; set; }
    }
}
