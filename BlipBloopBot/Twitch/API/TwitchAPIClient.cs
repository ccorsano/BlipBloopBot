using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.API
{
    public class TwitchAPIClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;

        private TwitchOAuthTokenResponse _tokenResponse;
        private string _oauthToken = null;
        private DateTime _oauthTokenExpiration = DateTime.MinValue;

        public TwitchAPIClient(string clientId, string clientSecret, IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public async Task AuthenticateAsync()
        {
            var uriBuilder = new UriBuilder("https://id.twitch.tv/oauth2/token");
            var param = new Dictionary<string, string>()
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "client_credentials" },
                { "scope", "chat:read chat:edit" }
            };
            var reqContent = new FormUrlEncodedContent(param);

            var result = await _httpClient.PostAsync(uriBuilder.Uri, reqContent);

            _tokenResponse = await JsonSerializer.DeserializeAsync<TwitchOAuthTokenResponse>(await result.Content.ReadAsStreamAsync());
            _oauthToken = _tokenResponse.AccessToken;
            _oauthTokenExpiration = DateTime.UtcNow.AddSeconds(_tokenResponse.ExpiresIn);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _oauthToken);
        }

        public async Task<HelixChannelSearchResult[]> SearchChannelsAsync(string channelQuery)
        {
            if (DateTime.UtcNow > _oauthTokenExpiration)
            {
                await AuthenticateAsync();
            }

            var uri = new Uri($"https://api.twitch.tv/helix/search/channels?query={channelQuery}");
            var result = await _httpClient.GetAsync(uri);
            if (result.IsSuccessStatusCode)
            {
                var response = await JsonSerializer.DeserializeAsync<HelixChannelsSearchResponse>(await result.Content.ReadAsStreamAsync());
                return response.Data;
            }

            return null;
        }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }
    }
}
