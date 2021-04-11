using Conceptoire.Twitch.Extensions;
using Conceptoire.Twitch.IRC;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopCommands.Commands
{
    public class ResponseCommandProcessor : IMessageProcessor
    {
        private readonly ILogger _logger;
        private bool _asReply;
        private string _message;

        public ResponseCommandProcessor(ILogger<ResponseCommandProcessor> logger)
        {
            _logger = logger;
        }

        public Task OnUpdateContext(IProcessorContext context)
        {
            _message = context.CommandOptions.Parameters.GetValueOrDefault("message");
            _asReply = bool.Parse(context.CommandOptions.Parameters.GetValueOrDefault("reply") ?? bool.FalseString);
            return Task.CompletedTask;
        }

        public void OnMessage(ParsedIRCMessage message, Action<OutgoingMessage> sendResponse)
        {
            string msgId = _asReply ? message.GetMessageIdTag() : null;
            var response = new OutgoingMessage
            {
                Message = _message,
                ReplyParentMessage = msgId,
            };
            sendResponse(response);
        }
    }
}
