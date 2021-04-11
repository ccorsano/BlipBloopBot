using System;
using System.Collections.Generic;

namespace Conceptoire.Twitch.Commands
{
    [Serializable]
    public class CommandOptions
    {
        public string Name { get; set; }
        public string[] Aliases { get; set; }
        public string Trigger { get; set; }
        public string Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}
