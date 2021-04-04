using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.Model.EventSub
{
    public class TwitchEventSubUserUpdateEvent : TwitchEventSubEvent
    {

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }

        [JsonPropertyName("email")]
        public string EMail { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
