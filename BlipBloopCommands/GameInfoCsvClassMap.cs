using Conceptoire.Twitch.Model;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopCommands
{
    public class GameInfoCsvClassMap : ClassMap<GameInfo>
    {
        public GameInfoCsvClassMap()
        {
            Map(gi => gi.TwitchCategoryId);
            Map(gi => gi.Language);
            Map(gi => gi.Name);
            Map(gi => gi.Synopsis);
            Map(gi => gi.Summary);
            Map(gi => gi.Source);
            Map(gi => gi.IGDBId).Optional();
            Map(gi => gi.SteamId).Optional();
        }
    }
}
