using Conceptoire.Twitch.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlipBloopCommands.Commands.GameSynopsis
{
    public interface ITwitchCategoryProvider
    {
        public Task<GameInfo> FetchChannelInfo(string categoryId, string language, CancellationToken cancellationToken = default);

        public event EventHandler<GameInfo> OnUpdate;
    }
}
