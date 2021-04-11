using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public class TwitchChatBot
    {
        private readonly TwitchChatClientBuilder _chatClientBuilder;
        private readonly ILogger _logger;

        public TwitchChatBot(TwitchChatClientBuilder chatBuilder, ILogger<TwitchChatBot> logger)
        {

        }
    }
}
