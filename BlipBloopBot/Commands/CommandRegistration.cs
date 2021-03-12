using BlipBloopBot.Twitch.IRC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Commands
{
    public class CommandRegistration
    {
        public string Name { get; set; }
        public Func<IMessageProcessor> Processor { get; set; }
    }
}
