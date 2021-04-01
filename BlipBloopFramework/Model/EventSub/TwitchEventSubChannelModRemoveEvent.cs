using System.Text.Json.Serialization;

namespace BlipBloopBot.Model.EventSub
{
    public class TwitchEventSubChannelModRemoveEvent : TwitchEventSubEvent
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }
    }
}
