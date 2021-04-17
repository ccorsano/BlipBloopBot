using Conceptoire.Twitch.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlipBloopBot.Storage
{
    public interface IGameLocalizationStore
    {
        Task SaveGameInfoAsync(GameInfo gameInfo, CancellationToken cancellationToken = default);
        Task<GameInfo> ResolveLocalizedGameInfoAsync(string language, string twitchCategoryId, CancellationToken cancellationToken = default);
    }
}
