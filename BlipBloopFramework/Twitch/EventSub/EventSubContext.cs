using BlipBloopBot.Model.EventSub;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.EventSub
{
    public class EventSubContext
    {
        public EventSubHeaders Headers { get; internal set; } = new EventSubHeaders();
        public bool IsValid { get; internal set; } = false;
        public TwitchEventSubSubscription Subscription { get; internal set; }
        public ILogger Logger { get; internal set; }
    }
}
