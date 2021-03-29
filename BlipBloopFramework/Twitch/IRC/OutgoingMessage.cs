using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.IRC
{
    public class OutgoingMessage
    {
        public string ReplyParentMessage { get; set; }
        public string Message { get; set; }
    }
}
