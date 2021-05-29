using Conceptoire.Twitch.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrainInterface
{
    public class ChannelState
    {
        public string LastTitle { get; set; }
        public string LastLanguage { get; set; }
        public string LastCategoryId { get; set; }
        public string LastCategoryName { get; set; }
        public string BroadcasterToken { get; set; }

        public List<HelixChannelModerator> Moderators { get; set; }

        public List<HelixChannelEditor> Editors { get; set; }
    }
}
