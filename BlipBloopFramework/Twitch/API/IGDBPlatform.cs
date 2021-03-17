using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.API
{
    public class IGDBPlatform
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("abbreviation")]
        public string Abbreviation { get; set; }

        [JsonPropertyName("alternative_name")]
        public string AlternativeName { get; set; }

        [JsonPropertyName("category")]
        public long Category { get; set; }

        [JsonPropertyName("created_at")]
        public long CreatedAt { get; set; }

        [JsonPropertyName("generation")]
        public long Generation { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("platform_logo")]
        public long PlatformLogo { get; set; }

        [JsonPropertyName("platform_family")]
        public long PlatformFamily { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("updated_at")]
        public long UpdatedAt { get; set; }

        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("versions")]
        public long[] Versions { get; set; }

        [JsonPropertyName("websites")]
        public long[] Websites { get; set; }

        [JsonPropertyName("checksum")]
        public Guid Checksum { get; set; }
    }
}
