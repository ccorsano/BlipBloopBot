﻿using Conceptoire.Twitch.Extensions;
using Conceptoire.Twitch.IRC;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopCommands.Commands
{
    public class ResponseCommandProcessor : BotCommandBase
    {
        private readonly ILogger _logger;
        private bool _asReply;
        private string _message;
        private ResponseCommandSettings _settings;

        public ResponseCommandProcessor(ILogger<ResponseCommandProcessor> logger)
        {
            _logger = logger;
        }

        public override bool CanHandleMessage(in ParsedIRCMessage message)
        {
            return false;
        }

        public override Task<IProcessorSettings> CreateSettings(Guid processorId)
        {
            _settings = new ResponseCommandSettings();
            return base.CreateSettings(processorId);
        }

        public override Task OnChangeSettings(IProcessorSettings settings)
        {
            throw new NotImplementedException();
        }

        public override Task OnUpdateContext(IProcessorContext context)
        {
            _message = _settings.Message;
            _asReply = false;
            return Task.CompletedTask;
        }

        public override void OnMessage(in ParsedIRCMessage message, Action<OutgoingMessage> sendResponse)
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
