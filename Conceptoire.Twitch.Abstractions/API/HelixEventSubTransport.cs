﻿using System;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixEventSubTransport
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("callback")]
        public Uri Callback { get; set; }

        /// <summary>
        /// Only outgoing
        /// </summary>
        [JsonPropertyName("secret")]
        public string Secret { get; set; }
    }
}
