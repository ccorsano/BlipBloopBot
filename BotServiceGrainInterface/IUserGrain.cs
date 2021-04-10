using Conceptoire.Twitch.API;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrain
{
    public interface IUserGrain : IGrainWithStringKey
    {
        public Task<bool> HasActiveChannel();

        public Task<bool> SetOAuthToken(string oauthToken);

        public Task<string> GetOAuthToken();

        public Task ActivateChannel();

        public Task AllowAsBot(string channelId);

        public Task RevokeAsBot(string channelId);

        public Task<HelixChannelInfo[]> GetChannelBotAllowList();
    }
}
