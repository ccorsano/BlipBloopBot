using System;
using System.Collections.Generic;

namespace Conceptoire.Twitch.Commands
{
    [Serializable]
    public class CommandOptions
    {
        public Guid Id { get; set; } = new Guid();
        public string[] Aliases { get; set; }
        public string Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}
