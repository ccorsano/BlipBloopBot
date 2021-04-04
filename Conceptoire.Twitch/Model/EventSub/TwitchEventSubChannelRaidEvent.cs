using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.Model.EventSub
{
    public class TwitchEventSubChannelRaidEvent : TwitchEventSubEvent
    {
        [JsonPropertyName("from_broadcaster_user_id")]
        public string FromBroadcasterUserId { get; set; }

        [JsonPropertyName("from_broadcaster_user_login")]
        public string FromBroadcasterUserLogin { get; set; }

        [JsonPropertyName("from_broadcaster_user_name")]
        public string FromBroadcasterUserName { get; set; }

        [JsonPropertyName("to_broadcaster_user_id")]
        public string ToBroadcasterUserId { get; set; }

        [JsonPropertyName("to_broadcaster_user_login")]
        public string ToBroadcasterUserLogin { get; set; }

        [JsonPropertyName("to_broadcaster_user_name")]
        public string ToBroadcasterUserName { get; set; }

        [JsonPropertyName("viewers")]
        public int Viewers { get; set; }
    }
}
