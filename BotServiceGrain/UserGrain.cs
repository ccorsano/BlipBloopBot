using BotServiceGrainInterface;
using BotServiceGrainInterface.Model;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Authentication;
using Conceptoire.Twitch.Constants;
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
                await SetOAuthToken(_profile.State.OAuthToken);
            }

            await base.OnActivateAsync();
        }

        public async Task<bool> SetOAuthToken(string oauthToken)
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

                if (_profile.State.HasActiveChannel)
                {
                    await SetRole(new UserRole
                    {
                        Role = ChannelRole.Broadcaster,
                        ChannelId = _tokenInfo.UserId,
                        ChannelName = _tokenInfo.Login,
                    });
                    var channelGrain = GrainFactory.GetGrain<IChannelGrain>(UserId);
                    await channelGrain.Activate(_profile.State.OAuthToken);
                }

                _profile.State.CurrentScopes = validated.Scopes.Where(s => TwitchConstants.ScopesValues.ContainsValue(s)).Select(s => TwitchConstants.ScopesValues.FirstOrDefault(kvp => kvp.Value == s).Key).ToArray();

                await _profile.WriteStateAsync();
            }
            else
            {
                _profile.State.CurrentScopes = new TwitchConstants.TwitchOAuthScopes[0];
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

            var channelInfo = await channelGrain.GetChannelInfo();

            await SetRole(new UserRole
            {
                Role = ChannelRole.Broadcaster,
                ChannelId = UserId,
                ChannelName = channelInfo.BroadcasterName,
            });

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

            if (!_profile.State.CurrentScopes.Contains(TwitchConstants.TwitchOAuthScopes.ChatEdit))
            {
                throw new Exception("Missing chat permission, authorize bot account first.");
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
            if (! _profile.State.CurrentScopes.Contains(TwitchConstants.TwitchOAuthScopes.ChatEdit))
            {
                return Task.FromResult(new HelixChannelInfo[0]);
            }
            return Task.FromResult(_profile.State.CanBeBotOnChannels.ToArray());
        }

        public Task<bool> HasActiveChannel()
        {
            return Task.FromResult(_profile.State.HasActiveChannel);
        }

        public Task SetRole(UserRole userRole)
        {
            if (! _profile.State.Roles.Contains(userRole))
            {
                _profile.State.Roles.Add(userRole);
            }
            return Task.CompletedTask;
        }

        public Task<UserRole[]> GetRoles()
        {
            return Task.FromResult(_profile.State.Roles.ToArray());
        }
    }
}
