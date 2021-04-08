using System;
using System.Collections.Generic;
using System.Text;

namespace Conceptoire.Twitch.Commands
{
    [Serializable]
    public class CommandMetadata
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
