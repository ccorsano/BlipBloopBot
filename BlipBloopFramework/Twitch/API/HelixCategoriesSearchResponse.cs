using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.API
{
    public class HelixCategoriesSearchResponse
    {
        [JsonPropertyName("data")]
        public HelixCategoriesSearchEntry[] Data { get; set; }

        [JsonPropertyName("pagination")]
        public HelixResponsePagination Pagination { get; set; }
    }
}
