using BotServiceGrainInterface;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using System;
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

        public UserGrain(
            [PersistentState("profile", "profileStore")] IPersistentState<ProfileState> profile,
            IHttpClientFactory httpClientFactory,
            ILogger<UserGrain> logger)
        {
            _profile = profile;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
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
                await _profile.WriteStateAsync();
            }

            return validated != null;
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
        }
    }
}
