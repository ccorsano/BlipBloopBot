using System.Collections.Generic;

namespace BlipBloopBot.Options
{
    public class CommandOptions
    {
        public string Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}
