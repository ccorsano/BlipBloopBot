﻿using BotServiceGrainInterface.Model;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Commands;
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
        public Task<bool> IsBotActive();
        public Task<bool> SetBotActivation(bool isActive);
        public Task<ChannelStaff> GetStaff();
        public Task AddCommand(CommandOptions options);
        public Task DeleteCommand(Guid commandId);
        public Task<CommandOptions[]> GetBotCommands();
        public Task UpdateBotCommands(CommandOptions[] commands);
        public Task<CommandMetadata[]> GetSupportedCommandTypes();
        public Task SetActiveBotAccount(string userId);
        public Task AllowBotAccount(BotAccountInfo accountInfo);
        public Task DisallowBotAccount(string userId);
        public Task<BotAccountInfo[]> GetAllowedBotAccounts();
        public Task ClearCustomizedCategoryDescription(string twitchCategory, string locale);
        public Task SetCustomizedCategoryDescription(CustomCategoryDescription categoryDescription);
        public Task<CustomCategoryDescription> GetCustomizedCategoryDescription(string twitchCategory, string locale);
        public Task<CustomCategoryDescription[]> GetCustomizedCategoryDescriptions();
    }
}
