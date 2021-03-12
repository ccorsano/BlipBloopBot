using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlipBloopBot.Steam.Model
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
