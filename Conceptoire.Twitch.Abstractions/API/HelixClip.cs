using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.API
{
    public class HelixClip
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("embed_url")]
        public string EmbedUrl { get; set; }

        [JsonIgnore]
        public Uri EmbedUri => string.IsNullOrEmpty(EmbedUrl) ? null : new Uri(EmbedUrl);

        [JsonPropertyName("broadcaster_id")]
        public string BroadcasterId { get; set; }

        [JsonPropertyName("broadcaster_name")]
        public string BroadcasterName { get; set; }

        [JsonPropertyName("creator_id")]
        public string CreatorId { get; set; }

        [JsonPropertyName("creator_name")]
        public string CreatorName { get; set; }

        [JsonPropertyName("video_id")]
        public string VideoId { get; set; }

        [JsonPropertyName("game_id")]
        public string GameId { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("view_count")]
        public long ViewCount { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonIgnore]
        public Uri ThumbnailUri => string.IsNullOrEmpty(ThumbnailUrl) ? null : new Uri(ThumbnailUrl);

        [JsonPropertyName("duration")]
        public double Duration { get; set; }
    }
}
