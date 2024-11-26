using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.PubSub
{
    public class BitsBadge : IPubSubDataObject
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }

        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; }

        [JsonPropertyName("channel_name")]
        public string ChannelName { get; set; }

        [JsonPropertyName("badge_tier")]
        public long BadgeTier { get; set; }

        [JsonPropertyName("chat_message")]
        public string ChatMessage { get; set; }

        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; set; }
    }
}
