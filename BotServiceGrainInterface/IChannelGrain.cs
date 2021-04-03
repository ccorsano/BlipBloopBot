using BlipBloopBot.Twitch.API;
using Orleans;
using System;
using System.Threading.Tasks;

namespace BotServiceGrain
{
    public interface IChannelGrain : IGrainWithStringKey
    {
        public Task<HelixChannelInfo> GetChannelInfo();
        public Task HandleBotCommand();
        public Task<bool> SetBotActivation(bool isActive);
    }
}
