﻿using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixChannelGetEditorsResponse
    {
        [JsonPropertyName("data")]
        public HelixChannelEditor[] Data { get; set; }
    }

    [JsonSerializable(typeof(HelixChannelGetEditorsResponse))]
    internal partial class HelixChannelGetEditorsResponseContext : JsonSerializerContext { }
}