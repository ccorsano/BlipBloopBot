using System.Text.Json.Serialization;

namespace BlipBloopBot.Model.EventSub
{
    public class TwitchEventSubCallbackPayload
    {
        [JsonPropertyName("subscription")]
        public TwitchEventSubSubscription Subscription { get; set; }

        [JsonPropertyName("event")]
        public TwitchEventSubEvent Event { get; set; }

        [JsonPropertyName("challenge")]
        public string Challenge { get; set; }
    }
}
