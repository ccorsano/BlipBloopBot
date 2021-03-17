using BlipBloopBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Storage
{
    public interface IGameLocalizationStore
    {
        Task<GameInfo> ResolveLocalizedGameInfo(string language, string twitchCategoryId);
    }
}
