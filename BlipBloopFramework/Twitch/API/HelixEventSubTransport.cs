using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.API
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
