using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.API
{
    public class IGDBExternalGame
    {
        [JsonPropertyName("id")]
        public uint Id { get; set; }

        [JsonPropertyName("category")]
        public IGDBExternalGameCategory Category { get; set; }
        
        [JsonPropertyName("created_at")]
        public UInt64 CreatedAt { get; set; }

        [JsonPropertyName("game")]
        public uint Game { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("uid")]
        public string UId { get; set; }

        [JsonPropertyName("updated_at")]
        public UInt64 UpdatedAt { get; set; }

        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("checksum")]
        public Guid Checksum { get; set; }
    }
}
