using Conceptoire.Twitch.IRC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopCommands.Commands.GameSynopsis
{
    public class GameSynopsisCommandSettings : IProcessorSettings
    {
        public string[] Aliases { get; set; }
        public bool AsReply { get; set; }

        public Task ReadAsync()
        {
            return Task.CompletedTask;
        }

        public Task WriteAsync()
        {
            return Task.CompletedTask;
        }
    }
}
