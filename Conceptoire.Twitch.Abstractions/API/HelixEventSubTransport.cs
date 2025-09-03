using System;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixEventSubTransport
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("callback")]
        public string Callback { get; set; }

        [JsonIgnore]
        public Uri CallbackUri => string.IsNullOrEmpty(Callback) ? null : new Uri(Callback);

        /// <summary>
        /// Only outgoing
        /// </summary>
        [JsonPropertyName("secret")]
        public string Secret { get; set; }
    }
}
