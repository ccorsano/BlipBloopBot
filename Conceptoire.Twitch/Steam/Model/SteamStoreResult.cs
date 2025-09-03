using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.Steam.Model
{
    public class SteamStoreResult : Dictionary<string, SteamStoreWrapper>
    {
    }

    public class SteamStoreWrapper
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public SteamStoreDetails Data { get; set; }
    }

    [JsonSerializable(typeof(SteamStoreResult))]
    internal sealed partial class SteamStoreResultContext : JsonSerializerContext { }
}
