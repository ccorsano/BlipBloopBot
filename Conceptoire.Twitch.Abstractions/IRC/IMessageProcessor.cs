using System;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public interface IMessageProcessor
    {
        Task Init(string channelName);
        void OnMessage(ParsedIRCMessage message, Action<OutgoingMessage> sendResponse);
    }
}
