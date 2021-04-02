using BotServiceGrainInterface;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrain
{
    public class UserGrain : Grain, IUserGrain
    {
        private readonly IPersistentState<ProfileState> _profile;
        private readonly ILogger _logger;

        public UserGrain([PersistentState("profile", "profileStore")] IPersistentState<ProfileState> profile, ILogger<UserGrain> logger)
        {
            _profile = profile;
            _logger = logger;
        }

        public Task SetOAuthToken(string oauthToken)
        {
            _profile.State.OAuthToken = oauthToken;
            return _profile.WriteStateAsync();
        }
    }
}
