using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixExtensionLiveChannel
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("broadcaster_id")]
        public string BroadcasterId { get; set; }
        [JsonPropertyName("broadcaster_name")]
        public string BroadcasterName { get; set; }
        [JsonPropertyName("game_name")]
        public string GameName { get; set; }
        [JsonPropertyName("game_id")]
        public string GameId { get; set; }
    }
}
