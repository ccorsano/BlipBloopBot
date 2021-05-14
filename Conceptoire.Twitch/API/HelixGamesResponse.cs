using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.API
{
    public class HelixGamesResponse
    {
        [JsonPropertyName("data")]
        public HelixCategoriesSearchEntry[] Data { get; set; }
    }
}
