using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public abstract class BotCommandBase : IMessageProcessor
    {
        public Guid Id { get; set; }

        public BotCommandBase()
        {

        }

        public abstract void OnMessage(ParsedIRCMessage message, Action<OutgoingMessage> sendResponse);

        public abstract Task OnUpdateContext(IProcessorContext context);
    }
}
