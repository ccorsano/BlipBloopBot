using System;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixCategoriesSearchEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("box_art_url")]
        public Uri BoxArtUri { get; set; }
    }
}
