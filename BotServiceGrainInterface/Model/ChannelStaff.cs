using System;
using Conceptoire.Twitch.API;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrainInterface.Model
{
    [Serializable]
    public class ChannelStaff
    {
        public HelixChannelEditor[] Editors { get; set; }
        public HelixChannelModerator[] Moderators { get; set; }
    }
}
