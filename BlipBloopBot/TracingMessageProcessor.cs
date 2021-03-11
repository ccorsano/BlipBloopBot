using BlipBloopBot.Twitch.IRC;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlipBloopBot
{
    class TracingMessageProcessor : IMessageProcessor
    {
        public void OnMessage(ParsedIRCMessage message)
        {
            Console.WriteLine($":{new string(message.Prefix)} {new string(message.Command)} : {new string(message.Trailing)}");
        }
    }
}
