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

        public DateTimeOffset ExpiresAt => _tokenExpirationTime;

        public bool AutoRenew => false;

        public TwitchConstants.TwitchOAuthScopes[] Scopes => _scopes;

        public Task<bool> Authenticate()
        {
            throw new NotImplementedException();
        }

        public Task AuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        public Task AuthenticateAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AuthenticateMessageAsync(HttpRequestMessage message)
        {
            throw new NotImplementedException();
        }

        public Task AuthenticateMessageAsync(HttpRequestMessage message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
