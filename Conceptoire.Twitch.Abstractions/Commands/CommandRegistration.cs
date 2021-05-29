using Conceptoire.Twitch.IRC;
using System;

namespace Conceptoire.Twitch.Commands
{
    [Serializable]
    public class CommandRegistration
    {
        public string Name => Metadata.Name;

        public CommandMetadata Metadata { get; set; }

        public Func<IMessageProcessor> Processor { get; set; }

        public Type ProcessorType { get; set; }
    }
}
