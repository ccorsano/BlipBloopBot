using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.API
{
    public class HelixVideoInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("stream_id")]
        public string StreamId { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonPropertyName("published_at")]
        public DateTimeOffset PublishedAt { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri => string.IsNullOrEmpty(Url) ? null : new Uri(Url);

        [JsonPropertyName("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonPropertyName("viewable")]
        public string Viewable { get; set; }

        [JsonPropertyName("view_count")]
        public long ViewCount { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("muted_segments")]
        public object MutedSegments { get; set; }
    }
}
