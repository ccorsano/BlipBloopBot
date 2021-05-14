using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrainInterface.Model
{
    public enum ChannelRole
    {
        Broadcaster,
        Editor,
        Moderator,
        None,
    }

    [Serializable]
    public struct UserRole
    {
        public ChannelRole Role { get; set; }
        public string ChannelId { get; set; }
        public string ChannelName { get; set; }
    }
}
