using Conceptoire.Twitch.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrainInterface
{
    [Serializable]
    public class ChannelBotSettingsState
    {
        public bool IsActive { get; set; }
        public Dictionary<string, CommandOptions> Commands { get; set; }
    }
}
