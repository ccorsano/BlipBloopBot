using System;
using System.Collections.Generic;
using System.Text;

namespace Conceptoire.Twitch.IRC
{
    public interface IProcessorContext
    {
        public string ChannelId { get; }
        public string ChannelName { get; }
        public string CategoryId { get; }
        public string Language { get; }
    }
}
