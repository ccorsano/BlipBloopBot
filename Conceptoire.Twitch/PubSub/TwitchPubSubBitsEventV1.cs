using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public class TwitchPubSubBitsEventV1
    {
        [JsonPropertyName("data")]
        public TwitchPubSubBitsEventData Data { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("message_type")]
        public string MessageType { get; set; }

        [JsonPropertyName("message_id")]
        public string MessageId { get; set; }
    }
}
