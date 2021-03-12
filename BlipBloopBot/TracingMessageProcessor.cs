using BlipBloopBot.Twitch.IRC;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BlipBloopBot
{
    /// <summary>
    /// Dummy message processor, logging messages
    /// </summary>
    class TracingMessageProcessor : IMessageProcessor
    {
        private readonly ILogger _logger;

        public TracingMessageProcessor(ILogger<TracingMessageProcessor> logger)
        {
            _logger = logger;
        }

        public void OnMessage(ParsedIRCMessage message)
        {
            _logger.LogInformation(":{prefix} {command} : {message}", new string(message.Prefix), new string(message.Command), new string(message.Trailing));

            List<string> commands = new List<string>();
            foreach(var botCommand in message.Trailing.ParseBotCommands('!'))
            {
                commands.Add(new string(botCommand));
            }
            if (commands.Count > 0)
            {
                _logger.LogWarning($"Commands: {string.Join(" ", commands)}");
            }
        }
    }
}
