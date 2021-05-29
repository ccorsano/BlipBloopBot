using Conceptoire.Twitch.Commands;
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
        public bool AsReply { get; set; } = true;
        public string BroadcasterId { get; set; }

        public void LoadFromOptions(CommandOptions options)
        {
            Aliases = options.Aliases.ToArray();
            if (options.Parameters.ContainsKey("AsReply"))
            {
                AsReply = bool.Parse(options.Parameters["AsReply"]);
            }
        }

        public void SaveToOptions(CommandOptions options)
        {
            options.Aliases = Aliases;
            options.Parameters["AsReply"] = AsReply ? bool.TrueString : bool.FalseString;
        }
    }
}
