using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace BlipBloopBot.Twitch.API
{
    public class HelixChannelsSearchResponse
    {
        [JsonPropertyName("data")]
        public HelixChannelSearchResult[] Data { get; set; }

        [JsonPropertyName("pagination")]
        public HelixResponsePagination Pagination { get; set; }
    }
}
