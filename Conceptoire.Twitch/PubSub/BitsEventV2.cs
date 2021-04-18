using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.PubSub
{
    public class BitsEventV2 : BitsEventV1
    {

        [JsonPropertyName("is_anonymous")]
        public bool IsAnonymous { get; set; }
    }
}
