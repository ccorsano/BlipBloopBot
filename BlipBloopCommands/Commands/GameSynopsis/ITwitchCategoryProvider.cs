using BlipBloopBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopCommands.Commands.GameSynopsis
{
    public interface ITwitchCategoryProvider
    {
        public Task<GameInfo> FetchChannelInfo(string channelId);

        public event EventHandler<GameInfo> OnUpdate;
    }
}
