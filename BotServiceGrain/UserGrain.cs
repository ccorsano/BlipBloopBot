using BotServiceGrainInterface;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BotServiceGrain
{
    public class UserGrain : Grain, IUserGrain
    {
        private readonly IPersistentState<ProfileState> _profile;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        private TwitchAPIClient _twitchAPIClient;
        private string UserId => GrainReference.GrainIdentity.PrimaryKeyString;
        private HelixValidateTokenResponse _tokenInfo;

        public UserGrain(
            [PersistentState("profile", "profileStore")] IPersistentState<ProfileState> profile,
            IHttpClientFactory httpClientFactory,
            ILogger<UserGrain> logger)
        {
            _profile = profile;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public override async Task OnActivateAsync()
        {
            if (_profile.RecordExists)
            {
                var authenticated = new AuthenticationBuilder()
                    .FromOAuthToken(_profile.State.OAuthToken)
                    .Build();
                _twitchAPIClient = new TwitchAPIClient(authenticated, _httpClientFactory, ServiceProvider.GetRequiredService<ILogger<TwitchAPIClient>>());
                _tokenInfo = await _twitchAPIClient.ValidateToken();
            }
            await base.OnActivateAsync();
        }

        async Task<bool> IUserGrain.SetOAuthToken(string oauthToken)
        {
            _profile.State.OAuthToken = oauthToken;

            var authenticated = new AuthenticationBuilder()
                .FromOAuthToken(oauthToken)
                .Build();
            _twitchAPIClient = new TwitchAPIClient(authenticated, _httpClientFactory, ServiceProvider.GetRequiredService<ILogger<TwitchAPIClient>>());
            var validated = await _twitchAPIClient.ValidateToken();

            if (validated != null)
            {
                _tokenInfo = validated;
                await _profile.WriteStateAsync();
            }

            return validated != null;
        }

        Task<string> IUserGrain.GetOAuthToken()
        {
            return Task.FromResult(_profile.State.OAuthToken);
        }

        async Task IUserGrain.ActivateChannel()
        {
            var validatedToken = await _twitchAPIClient.ValidateToken();
            if (validatedToken == null || validatedToken.UserId != UserId)
            {
                throw new InvalidOperationException("Missing active user token");
            }

            var channelGrain = GrainFactory.GetGrain<IChannelGrain>(UserId);
            await channelGrain.Activate(_profile.State.OAuthToken);

            _profile.State.HasActiveChannel = true;
            await _profile.WriteStateAsync();
        }

        async Task IUserGrain.AllowAsBot(string channelId)
        {
            var validatedToken = await _twitchAPIClient.ValidateToken();
            if (validatedToken == null || validatedToken.UserId != UserId)
            {
                throw new InvalidOperationException("Missing active user token");
            }

            var channel = await _twitchAPIClient.GetChannelInfoAsync(channelId);
            if (channel == null)
            {
                _logger.LogError("AllowAsBot: invalid channel provider");
                throw new ArgumentException("Cannot find channel {channelId}", channelId);
            }

            if (! _profile.State.CanBeBotOnChannels.Any(c => c.BroadcasterId == channelId))
            {
                var channelInfo = await _twitchAPIClient.GetChannelInfoAsync(channelId);
                _profile.State.CanBeBotOnChannels.Add(channelInfo);
            }

            var channelGrain = GrainFactory.GetGrain<IChannelGrain>(channelId);
            await channelGrain.AllowBotAccount(
                new BotServiceGrainInterface.Model.BotAccountInfo {
                    UserId = UserId,
                    UserLogin = _tokenInfo.Login,
            });

            await _profile.WriteStateAsync();
        }

        async Task IUserGrain.RevokeAsBot(string channelId)
        {
            if (_profile.State.CanBeBotOnChannels.Any(c => c.BroadcasterId == channelId))
            {
                var channelGrain = GrainFactory.GetGrain<IChannelGrain>(channelId);
                await channelGrain.DisallowBotAccount(UserId);

                var channelInfo = await _twitchAPIClient.GetChannelInfoAsync(channelId);
                _profile.State.CanBeBotOnChannels.RemoveAll(c => c.BroadcasterId == channelId);
            }
        }

        Task<HelixChannelInfo[]> IUserGrain.GetChannelBotAllowList()
        {
            return Task.FromResult(_profile.State.CanBeBotOnChannels.ToArray());
        }

        public Task<bool> HasActiveChannel()
        {
            return Task.FromResult(_profile.State.HasActiveChannel);
        }
    }
}
