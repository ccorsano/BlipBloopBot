using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public class TwitchPubSubBitsEventv2 : TwitchPubSubBitsEventV1
    {

        [JsonPropertyName("is_anonymous")]
        public bool IsAnonymous { get; set; }
    }
}
