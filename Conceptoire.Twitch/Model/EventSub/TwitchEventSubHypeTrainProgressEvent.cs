using System.Text.Json.Serialization;

namespace BlipBloopBot.Model.EventSub
{
    public class TwitchEventSubHypeTrainProgressEvent : TwitchEventSubEvent
    {
        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("progress")]
        public int Progress { get; set; }

        [JsonPropertyName("goal")]
        public int Goal { get; set; }

        [JsonPropertyName("top_contributions")]
        public TwitchEventSubHypeTrainContributor[] TopContributions { get; set; }

        [JsonPropertyName("last_contribution")]
        public TwitchEventSubHypeTrainContributor LastContribution { get; set; }

        [JsonPropertyName("started_at")]
        public int StartedAt { get; set; }

        [JsonPropertyName("expires_at")]
        public int ExpiresAt { get; set; }
    }
}
