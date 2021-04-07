using BotServiceGrainInterface.Model;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Options;
using Orleans;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotServiceGrain
{
    public interface IChannelGrain : IGrainWithStringKey
    {
        public Task Activate(string userToken);
        public Task OnChannelUpdate(HelixChannelInfo info);
        public Task<HelixChannelInfo> GetChannelInfo();
        public Task HandleBotCommand();
        public Task<bool> IsBotActive();
        public Task<bool> SetBotActivation(bool isActive);
        public Task<ChannelStaff> GetStaff();
        public Task<Dictionary<string, CommandOptions>> GetBotCommands();
    }
}
