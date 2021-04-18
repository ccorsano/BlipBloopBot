using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public class TwitchPubSubBadgeEntitlement
    {
        [JsonPropertyName("new_version")]
        public long NewVersion { get; set; }

        [JsonPropertyName("previous_version")]
        public long PreviousVersion { get; set; }
    }
}
