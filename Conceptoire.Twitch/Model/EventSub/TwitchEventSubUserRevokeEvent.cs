using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.Model.EventSub
{
    public class TwitchEventSubUserRevokeEvent : TwitchEventSubEvent
    {
        [JsonPropertyName("client_id")]
        public string clientId { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }
    }
}
