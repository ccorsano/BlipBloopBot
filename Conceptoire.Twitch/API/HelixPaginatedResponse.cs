using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.API
{
    public class HelixPaginatedResponse<TEntry> where TEntry:class
    {
        [JsonPropertyName("data")]
        public TEntry[] Data { get; set; }

        [JsonPropertyName("pagination")]
        public HelixResponsePagination Pagination { get; set; }
    }
}
