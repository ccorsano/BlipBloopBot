using System;
using System.Collections.Generic;
using System.Text;

namespace BlipBloopBot.Twitch.IRC
{
    public interface IMessageProcessor
    {
        void OnMessage(ParsedIRCMessage message);
    }
}
