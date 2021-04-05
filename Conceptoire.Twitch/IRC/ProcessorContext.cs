using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public class ProcessorContext : IProcessorContext
    {
        public string ChannelId { get; set; }

        public string ChannelName { get; set; }

        public string Language { get; set; }

        public string CategoryId { get; set; }
    }
}
