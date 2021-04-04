using System;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.Model.EventSub
{
    public class TwitchEventSubChannelBanEvent : TwitchEventSubEvent
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }

        [JsonPropertyName("moderator_user_id")]
        public string ModeratorUserId { get; set; }

        [JsonPropertyName("moderator_user_login")]
        public string ModeratorUserLogin { get; set; }

        [JsonPropertyName("moderator_user_name")]
        public string ModeratorUserName { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("ends_at")]
        public DateTimeOffset EndsAt { get; set; }

        [JsonPropertyName("is_permanent")]
        public bool IsPermanent { get; set; }
    }
}
