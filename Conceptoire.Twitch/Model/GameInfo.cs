using BlipBloopBot.Twitch.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Model
{
    public class GameInfo
    {
        public string TwitchCategoryId { get; set; }
        public string Language { get; set; }
        public string Name { get; set; }
        public string Synopsis { get; set; }
        public string Summary { get; set; }
        public string Source { get; set; }
        public ulong? IGDBId { get; set; }
    }
}
