﻿using System;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    [Serializable]
    public class HelixChannelInfo
    {
        [JsonPropertyName("broadcaster_id")]
        public string BroadcasterId { get; set; }

        [JsonPropertyName("broadcaster_name")]
        public string BroadcasterName { get; set; }

        [JsonPropertyName("broadcaster_language")]
        public string BroadcasterLanguage { get; set; }

        [JsonPropertyName("game_id")]
        public string GameId { get; set; }

        [JsonPropertyName("game_name")]
        public string GameName { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
}