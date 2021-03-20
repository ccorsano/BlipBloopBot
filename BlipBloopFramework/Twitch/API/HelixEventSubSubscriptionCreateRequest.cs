using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.API
{
    public class HelixEventSubSubscriptionCreateRequest
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("condition")]
        public Dictionary<string, string> Condition { get; set; }

        [JsonPropertyName("transport")]
        public HelixEventSubTransport Transport { get; set; }
    }
}
