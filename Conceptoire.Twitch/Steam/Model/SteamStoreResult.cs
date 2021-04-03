using System.Collections.Generic;

namespace Conceptoire.Twitch.Steam.Model
{
    public class SteamStoreResult : Dictionary<string, SteamStoreWrapper>
    {
    }

    public class SteamStoreWrapper
    {
        public bool Success { get; set; }
        public SteamStoreDetails Data { get; set; }
    }
}
