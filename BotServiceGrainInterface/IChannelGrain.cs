using Orleans;
using System;
using System.Threading.Tasks;

namespace BotServiceGrain
{
    public interface IChannelGrain : IGrainWithStringKey
    {
        public Task<string> GetChannelInfo();
        public Task HandleBotCommand();
    }
}
