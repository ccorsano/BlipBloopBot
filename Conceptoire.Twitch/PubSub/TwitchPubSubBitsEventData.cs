using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public class TwitchPubSubBitsEventData
    {
        [JsonPropertyName("user_name")]
        public string UserName { get; set; }

        [JsonPropertyName("channel_name")]
        public string ChannelName { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; }

        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; set; }

        [JsonPropertyName("chat_message")]
        public string ChatMessage { get; set; }

        [JsonPropertyName("bits_used")]
        public long BitsUsed { get; set; }

        [JsonPropertyName("total_bits_used")]
        public long TotalBitsUsed { get; set; }

        [JsonPropertyName("context")]
        public string Context { get; set; }

        [JsonPropertyName("badge_entitlement")]
        public TwitchPubSubBadgeEntitlement BadgeEntitlement { get; set; }
    }
}
