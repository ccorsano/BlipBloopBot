using BotServiceGrainInterface.Model;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrainInterface
{
    [Serializable]
    public class ProfileState
    {
        public bool HasActiveChannel { get; set; } = false;
        public string OAuthToken { get; set; }
        public TwitchConstants.TwitchOAuthScopes[] CurrentScopes { get; set; } = new TwitchConstants.TwitchOAuthScopes[0];
        public List<HelixChannelInfo> CanBeBotOnChannels { get; set; } = new List<HelixChannelInfo>();
        public HashSet<UserRole> Roles { get; set; } = new HashSet<UserRole>();
    }
}
