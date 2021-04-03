using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.Model.EventSub
{
    public class TwitchEventSubHypeTrainBeginEvent : TwitchEventSubEvent
    {
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

    public class TwitchEventSubHypeTrainContributor
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
