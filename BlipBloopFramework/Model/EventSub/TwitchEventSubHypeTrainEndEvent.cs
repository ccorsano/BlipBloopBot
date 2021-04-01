using System.Text.Json.Serialization;

namespace BlipBloopBot.Model.EventSub
{
    public class TwitchEventSubHypeTrainEndEvent : TwitchEventSubEvent
    {
        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("top_contributions")]
        public TwitchEventSubHypeTrainContributor[] TopContributions { get; set; }

        [JsonPropertyName("started_at")]
        public int StartedAt { get; set; }

        [JsonPropertyName("expires_at")]
        public int ExpiresAt { get; set; }

        [JsonPropertyName("cooldown_ends_at")]
        public int CoolDownEndsAt { get; set; }
    }
}
