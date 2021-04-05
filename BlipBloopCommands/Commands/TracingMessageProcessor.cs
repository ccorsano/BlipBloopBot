using Conceptoire.Twitch.IRC;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlipBloopBot.Commands
{
    /// <summary>
    /// Dummy message processor, logging messages
    /// </summary>
    public class TracingMessageProcessor : IMessageProcessor
    {
        private readonly ILogger _logger;

        public TracingMessageProcessor(ILogger<TracingMessageProcessor> logger)
        {
            _logger = logger;
        }

        public Task OnUpdateContext(IProcessorContext context)
        {
            return Task.CompletedTask;
        }

        public void OnMessage(ParsedIRCMessage message, Action<OutgoingMessage> _)
        {
            _logger.LogInformation(":{prefix} {command} : {message}", new string(message.Prefix), new string(message.Command), new string(message.Trailing));

            List<string> commands = new List<string>();
            foreach(var botCommand in message.Trailing.ParseBotCommands('!'))
            {
                commands.Add(botCommand);
            }
            if (commands.Count > 0)
            {
                _logger.LogWarning($"Commands: {string.Join(" ", commands)}");
            }
        }
    }
}
