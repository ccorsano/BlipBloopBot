using System;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixChannelSearchResult
    {
        [JsonPropertyName("broadcaster_language")]
        public string BroadcasterLanguage { get; set; }

        [JsonPropertyName("broadcaster_login")]
        public string BroadcasterLogin { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("game_id")]
        public string GameId { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("is_live")]
        public bool IsLive { get; set; }

        [JsonPropertyName("tags_ids")]
        public Guid[] TagsIds { get; set; }

        [JsonPropertyName("thumbnail_url")]
        public Uri ThumbnailUrl { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("started_at")]
        [JsonConverter(typeof(JsonDateTimeOffsetConverter))]
        public DateTimeOffset? StartedAt { get; set; }
    }
}