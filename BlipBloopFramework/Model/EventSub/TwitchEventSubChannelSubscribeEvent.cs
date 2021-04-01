using System.Text.Json.Serialization;

namespace BlipBloopBot.Model.EventSub
{
    public class TwitchEventSubChannelSubscribeEvent : TwitchEventSubEvent
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }

        [JsonPropertyName("tier")]
        public string Tier { get; set; }

        [JsonPropertyName("is_gift")]
        public bool IsGift { get; set; }
    }
}
