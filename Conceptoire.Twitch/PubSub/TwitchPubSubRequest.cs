using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.PubSub
{
    public class TwitchPubSubRequest
    {
        public TwitchPubSubRequest()
        {
            Type = "LISTEN";
            Nonce = Guid.NewGuid().ToString();
        }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("nonce")]
        public string Nonce { get; set; }

        [JsonPropertyName("data")]
        public TwitchPubSubRequestData Data { get; set; }
    }

    public class TwitchPubSubRequestData
    {
        [JsonPropertyName("topics")]
        public string[] Topics { get; set; }

        [JsonPropertyName("auth_token")]
        public string AuthToken { get; set; }

    }
}
