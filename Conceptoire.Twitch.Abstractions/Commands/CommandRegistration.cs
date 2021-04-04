using Conceptoire.Twitch.IRC;
using System;

namespace Conceptoire.Twitch.Commands
{
    public class CommandRegistration
    {
        public string Name { get; set; }
        public Func<IMessageProcessor> Processor { get; set; }
    }
}
