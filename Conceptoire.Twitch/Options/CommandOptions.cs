using System.Collections.Generic;

namespace Conceptoire.Twitch.Options
{
    public class CommandOptions
    {
        public string Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}
