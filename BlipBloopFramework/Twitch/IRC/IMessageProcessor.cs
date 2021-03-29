using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.IRC
{
    public interface IMessageProcessor
    {
        Task Init(string channelName);
        void OnMessage(ParsedIRCMessage message, Action<OutgoingMessage> sendResponse);
    }
}
