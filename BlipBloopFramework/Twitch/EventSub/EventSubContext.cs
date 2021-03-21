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
    }
}
