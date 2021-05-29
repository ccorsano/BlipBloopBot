using Conceptoire.Twitch.Commands;
using System.Collections.Generic;

namespace Conceptoire.Twitch.Options
{
    public class ChannelOptions
    {
        public string BroadcasterLogin { get; set; }
        public Dictionary<string, CommandOptions> Commands { get; set; }
    }
}
