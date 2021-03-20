using System.Text.Json.Serialization;

namespace BlipBloopBot.Model.EventSub
{
    public class TwitchEventSubCallbackPayload
    {
        [JsonPropertyName("subscription")]
        public TwitchEventSubSubscription Subscription { get; set; }

        [JsonPropertyName("challenge")]
        public string Challenge { get; set; }
    }

    public class TwitchEventSubCallbackPayload<TEvent> : TwitchEventSubCallbackPayload where TEvent : TwitchEventSubEvent
    {
        [JsonPropertyName("event")]
        public TEvent Event { get; set; }
    }
}
