using BlipBloopBot.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.Authentication
{
    public class OAuthAuthenticated : IAuthenticated
    {
        private string _oauthToken;
        private TwitchConstants.TwitchOAuthScopes[] _scopes;
        private DateTimeOffset _tokenExpirationTime;

        internal OAuthAuthenticated(string oauthToken, HashSet<TwitchConstants.TwitchOAuthScopes> scopes)
        {
            _oauthToken = oauthToken;
            _scopes = scopes.ToArray();
            _tokenExpirationTime = DateTimeOffset.MinValue;
        }

        public TwitchConstants.TwitchOAuthScopes[] Scopes => _scopes;

        public string Token => _oauthToken;

        public DateTimeOffset ExpiresAt => _tokenExpirationTime;

        public bool AutoRenew => false;

        public Task AuthenticateAsync() => AuthenticateAsync(CancellationToken.None);

        public Task AuthenticateAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AuthenticateMessageAsync(HttpRequestMessage message) => AuthenticateMessageAsync(message, CancellationToken.None);

        public Task AuthenticateMessageAsync(HttpRequestMessage message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
