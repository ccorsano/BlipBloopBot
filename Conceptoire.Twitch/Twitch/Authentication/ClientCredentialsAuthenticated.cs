using BlipBloopBot.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.Authentication
{
    public class ClientCredentialsAuthenticated : IAuthenticated
    {
        private HttpClient _httpClient;
        private string _clientId;
        private string _clientSecret;
        private TwitchConstants.TwitchOAuthScopes[] _scopes;
        TwitchOAuthTokenResponse _tokenResponse;
        private DateTimeOffset _tokenExpiration;

        internal ClientCredentialsAuthenticated(string clientId, string clientSecret, HashSet<TwitchConstants.TwitchOAuthScopes> scopes)
            :this(new HttpClient(), clientId, clientSecret, scopes) { }

        internal ClientCredentialsAuthenticated(IHttpMessageHandlerFactory httpMessageHandlerFactory, string clientId, string clientSecret, HashSet<TwitchConstants.TwitchOAuthScopes> scopes)
            : this(new HttpClient(httpMessageHandlerFactory.CreateHandler()), clientId, clientSecret, scopes) { }

        internal ClientCredentialsAuthenticated(IHttpClientFactory httpClientFactory, string clientId, string clientSecret, HashSet<TwitchConstants.TwitchOAuthScopes> scopes)
            : this(httpClientFactory.CreateClient(), clientId, clientSecret, scopes) { }

        internal ClientCredentialsAuthenticated(HttpMessageHandler httpMessageHandler, string clientId, string clientSecret, HashSet<TwitchConstants.TwitchOAuthScopes> scopes)
            : this(new HttpClient(httpMessageHandler), clientId, clientSecret, scopes) { }

        internal ClientCredentialsAuthenticated(HttpClient httpClient, string clientId, string clientSecret, HashSet<TwitchConstants.TwitchOAuthScopes> scopes)
        {
            _httpClient = httpClient;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _scopes = scopes.ToArray();
        }

        public string Token => _tokenResponse?.AccessToken;

        public DateTimeOffset ExpiresAt => _tokenExpiration;

        public bool AutoRenew => true;

        public TwitchConstants.TwitchOAuthScopes[] Scopes => _scopes;

        public string Login => null;

        public Task AuthenticateAsync() => AuthenticateAsync(CancellationToken.None);

        public async Task AuthenticateAsync(CancellationToken cancellationToken)
        {
            if (_tokenExpiration > DateTimeOffset.UtcNow)
            {
                return;
            }

            var uriBuilder = new UriBuilder("https://id.twitch.tv/oauth2/token");
            var param = new Dictionary<string, string>()
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "client_credentials" },
                { "scope", string.Join(" ", _scopes.Select(s => TwitchConstants.ScopesValues[s])) }
            };
            var reqContent = new FormUrlEncodedContent(param);

            var result = await _httpClient.PostAsync(uriBuilder.Uri, reqContent, cancellationToken);
            result.EnsureSuccessStatusCode();

            _tokenResponse = await JsonSerializer.DeserializeAsync<TwitchOAuthTokenResponse>(await result.Content.ReadAsStreamAsync());
            _tokenExpiration = DateTimeOffset.UtcNow.AddSeconds(_tokenResponse.ExpiresIn);
        }

        public Task AuthenticateMessageAsync(HttpRequestMessage message) => AuthenticateMessageAsync(message, CancellationToken.None);

        public async Task AuthenticateMessageAsync(HttpRequestMessage message, CancellationToken cancellationToken)
        {
            await AuthenticateAsync(cancellationToken);

            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenResponse.AccessToken);
            message.Headers.Add("client-id", _clientId);
        }
    }
}
