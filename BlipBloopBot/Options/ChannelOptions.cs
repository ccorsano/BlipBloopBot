using System.Collections.Generic;

namespace BlipBloopBot.Options
{
    public class ChannelOptions
    {
        public string BroadcasterLogin { get; set; }
        public Dictionary<string, CommandOptions> Commands { get; set; }
    }
}
