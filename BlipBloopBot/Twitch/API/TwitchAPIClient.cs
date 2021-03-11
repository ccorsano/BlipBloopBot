using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;

        private string _clientId;
        private string _clientSecret;

        private TwitchOAuthTokenResponse _tokenResponse;
        private string _oauthToken = null;
        private DateTime _oauthTokenExpiration = DateTime.MinValue;

        public TwitchAPIClient(IHttpClientFactory factory, ILogger<TwitchAPIClient> logger)
        {
            _httpClient = factory.CreateClient();
            _logger = logger;
        }

        public async Task AuthenticateAsync(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;

            var uriBuilder = new UriBuilder("https://id.twitch.tv/oauth2/token");
            var param = new Dictionary<string, string>()
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "client_credentials" },
                //{ "scope", "chat:read chat:edit" }
            };
            var reqContent = new FormUrlEncodedContent(param);

            var result = await _httpClient.PostAsync(uriBuilder.Uri, reqContent);

            _tokenResponse = await JsonSerializer.DeserializeAsync<TwitchOAuthTokenResponse>(await result.Content.ReadAsStreamAsync());
            _oauthToken = _tokenResponse.AccessToken;
            _oauthTokenExpiration = DateTime.UtcNow.AddSeconds(_tokenResponse.ExpiresIn);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _oauthToken);
            _httpClient.DefaultRequestHeaders.Add("client-id", _clientId);
        }

        public async Task<HelixChannelSearchResult[]> SearchChannelsAsync(string channelQuery)
        {
            if (DateTime.UtcNow > _oauthTokenExpiration)
            {
                await AuthenticateAsync(_clientId, _clientSecret);
            }

            var uri = new Uri($"https://api.twitch.tv/helix/search/channels?query={channelQuery}");
            var result = await _httpClient.GetAsync(uri);
            if (result.IsSuccessStatusCode)
            {
                var response = await JsonSerializer.DeserializeAsync<HelixChannelsSearchResponse>(await result.Content.ReadAsStreamAsync());
                return response.Data;
            }
            else
            {
                var response = await result.Content.ReadAsStringAsync();
                _logger.LogError(response);
            }

            return null;
        }

        public async Task<HelixChannelInfo> GetChannelInfoAsync(string broadcasterId)
        {
            if (DateTime.UtcNow > _oauthTokenExpiration)
            {
                await AuthenticateAsync(_clientId, _clientSecret);
            }

            var uri = new Uri($"https://api.twitch.tv/helix/channels?broadcaster_id={broadcasterId}");
            var result = await _httpClient.GetAsync(uri);
            if (result.IsSuccessStatusCode)
            {
                var response = await JsonSerializer.DeserializeAsync<HelixGetChannelInfoResponse>(await result.Content.ReadAsStreamAsync());
                return response.Data?[0] ?? null;
            }
            else
            {
                var response = await result.Content.ReadAsStringAsync();
                _logger.LogError(response);
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
