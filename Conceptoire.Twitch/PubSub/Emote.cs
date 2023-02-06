using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.PubSub
{
    public class Emote
    {
        [JsonPropertyName("start")]
        public long Start { get; set; }

        [JsonPropertyName("end")]
        public long End { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}
