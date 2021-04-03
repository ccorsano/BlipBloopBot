using System;
using System.Text.Json.Serialization;

namespace BlipBloopBot.Model.EventSub
{
    public class TwitchEventSubStreamOnlineEvent : TwitchEventSubEvent
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("started_at")]
        public DateTimeOffset StartedAt { get; set; }
    }
}
