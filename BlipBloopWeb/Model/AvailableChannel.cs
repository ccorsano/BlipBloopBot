using BotServiceGrainInterface.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlipBloopWeb.Model
{
    public class AvailableChannel
    {
        public string BroadcasterId { get; set; }
        public string BroadcasterName { get; set; }
        public ChannelRole UserRole { get; set; }
    }
}
