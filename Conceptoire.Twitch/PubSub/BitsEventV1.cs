using Conceptoire.Twitch.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.PubSub
{
    public class BitsEventV1 : IPubSubDataObject
    {
        [JsonPropertyName("data")]
        public BitsEventData Data { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("message_type")]
        public string MessageType { get; set; }

        [JsonPropertyName("message_id")]
        public string MessageId { get; set; }
    }
}
