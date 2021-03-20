using System.Text.Json.Serialization;

namespace BlipBloopBot.Model.EventSub
{
    public class TwitchEventSubChannelUpdateEvent : TwitchEventSubEvent
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("category_id")]
        public string CategoryId { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; }

        [JsonPropertyName("is_mature")]
        public bool IsMature { get; set; }
    }
}
