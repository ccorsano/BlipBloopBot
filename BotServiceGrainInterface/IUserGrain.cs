using BotServiceGrainInterface.Model;
using Conceptoire.Twitch.API;
using Orleans;
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

        public Task SetRole(UserRole userRole);

        public Task<UserRole[]> GetRoles();
    }
}
