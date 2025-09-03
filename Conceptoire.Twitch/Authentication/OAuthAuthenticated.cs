using Conceptoire.Twitch.Authentication;
using Conceptoire.Twitch.Constants;
using Conceptoire.Twitch.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.Authentication
{
    public class OAuthAuthenticated : IBotAuthenticated
    {
        private HttpClient _httpClient;
        private string _oauthToken;
        private string _clientId;
        private string _login;
        private TwitchConstants.TwitchOAuthScopes[] _scopes;
        private DateTimeOffset _tokenExpirationTime;


        internal OAuthAuthenticated(string oauthToken, HashSet<TwitchConstants.TwitchOAuthScopes> scopes)
            : this(new HttpClient(), oauthToken, scopes) { }

        internal OAuthAuthenticated(IHttpMessageHandlerFactory httpMessageHandlerFactory, string oauthToken, HashSet<TwitchConstants.TwitchOAuthScopes> scopes)
            : this(new HttpClient(httpMessageHandlerFactory.CreateHandler()), oauthToken, scopes) { }

        internal OAuthAuthenticated(IHttpClientFactory httpClientFactory, string oauthToken, HashSet<TwitchConstants.TwitchOAuthScopes> scopes)
            : this(httpClientFactory.CreateClient(), oauthToken, scopes) { }

        internal OAuthAuthenticated(HttpMessageHandler httpMessageHandler, string oauthToken, HashSet<TwitchConstants.TwitchOAuthScopes> scopes)
            : this(new HttpClient(httpMessageHandler), oauthToken, scopes) { }

        internal OAuthAuthenticated(HttpClient httpClient, string oauthToken, HashSet<TwitchConstants.TwitchOAuthScopes> scopes)
        {
            _httpClient = httpClient;
            _oauthToken = oauthToken;
            _scopes = scopes.ToArray();
            _tokenExpirationTime = DateTimeOffset.MinValue;
        }

        public TwitchConstants.TwitchOAuthScopes[] Scopes => _scopes;

        public string Token => _oauthToken;

        public DateTimeOffset ExpiresAt => _tokenExpirationTime;

        public bool AutoRenew => false;

        public string Login => _login;

        public Task AuthenticateAsync() => AuthenticateAsync(CancellationToken.None);

        public async Task AuthenticateAsync(CancellationToken cancellationToken)
        {
            if (_tokenExpirationTime > DateTimeOffset.UtcNow)
            {
                return;
            }

            var uri = "https://id.twitch.tv/oauth2/validate";
            var message = new HttpRequestMessage(HttpMethod.Get, uri);
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _oauthToken);

            var result = await _httpClient.SendAsync(message, cancellationToken);
            var response = await JsonSerializer.DeserializeAsync(await result.Content.ReadAsStreamAsync(), HelixJsonContext.Default.HelixValidateTokenResponse);

            if (response.ExpiresIn < 0)
            {
                throw new ArgumentOutOfRangeException("OAuthToken", "The provided token has expired");
            }

            _clientId = response.ClientId;
            _tokenExpirationTime = DateTimeOffset.UtcNow.Add(TimeSpan.FromSeconds(response.ExpiresIn));
            if (response.Scopes != null)
            {
                _scopes = response.Scopes
                    .Where(s => TwitchConstants.ScopesValues.ContainsValue(s))
                    .Select(s => TwitchConstants.ScopesValues.First(kvp => kvp.Value == s).Key).ToArray();
            }
            _login = response.Login;
        }

        public Task AuthenticateMessageAsync(HttpRequestMessage message)
            => AuthenticateMessageAsync(message, CancellationToken.None);

        public Task AuthenticateMessageAsync(HttpRequestMessage message, CancellationToken cancellationToken)
            => AuthenticateMessageAsync(message, true, cancellationToken);

        public Task AuthenticateMessageAsync(HttpRequestMessage message, bool setClientIdHeader)
            => AuthenticateMessageAsync(message, setClientIdHeader, CancellationToken.None);

        public async Task AuthenticateMessageAsync(HttpRequestMessage message, bool setClientIdHeader, CancellationToken cancellationToken)
        {
            await AuthenticateAsync(cancellationToken);

            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _oauthToken);
            
            if (setClientIdHeader)
            {
                message.Headers.Add("Client-ID", _clientId);
            }
        }
    }
}
