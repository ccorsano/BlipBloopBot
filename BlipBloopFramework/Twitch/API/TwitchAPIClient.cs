using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
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

        public Task<string> AuthenticateAsync() => AuthenticateAsync(CancellationToken.None);

        public async Task<string> AuthenticateAsync(CancellationToken cancellationToken)
        {
            if (DateTime.UtcNow > _oauthTokenExpiration)
            {
                await AuthenticateAsync(_clientId, _clientSecret, cancellationToken);
            }
            return _oauthToken;
        }

        public Task AuthenticateAsync(string clientId, string clientSecret) => AuthenticateAsync(clientId, clientSecret, CancellationToken.None);

        public async Task AuthenticateAsync(string clientId, string clientSecret, CancellationToken cancellationToken)
        {
            if ((_clientId == clientId || _clientSecret == clientSecret) && _oauthTokenExpiration > DateTime.UtcNow)
            {
                return;
            }

            _clientId = clientId;
            _clientSecret = clientSecret;

            var uriBuilder = new UriBuilder("https://id.twitch.tv/oauth2/token");
            var param = new Dictionary<string, string>()
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "client_credentials" },
            };
            var reqContent = new FormUrlEncodedContent(param);

            var result = await _httpClient.PostAsync(uriBuilder.Uri, reqContent, cancellationToken);

            _tokenResponse = await JsonSerializer.DeserializeAsync<TwitchOAuthTokenResponse>(await result.Content.ReadAsStreamAsync());
            _oauthToken = _tokenResponse.AccessToken;
            _oauthTokenExpiration = DateTime.UtcNow.AddSeconds(_tokenResponse.ExpiresIn);

            lock (_httpClient)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _oauthToken);
                if (!_httpClient.DefaultRequestHeaders.Contains("client-id"))
                {
                    _httpClient.DefaultRequestHeaders.Add("client-id", _clientId);
                }
            }
        }

        public async Task<HelixChannelSearchResult[]> SearchChannelsAsync(string channelQuery)
        {
            await AuthenticateAsync();

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
            await AuthenticateAsync();

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

        public async IAsyncEnumerable<HelixCategoriesSearchEntry> EnumerateTwitchCategoriesAsync()
        {
            await AuthenticateAsync();

            HelixCategoriesSearchResponse response = null;
            uint paginationRound = 1;
            int totalItems = 0;
            do
            {
                var uri = "https://api.twitch.tv/helix/search/categories";
                _logger.LogDebug("Fetching categories from Twitch API, pagination {paginationCursor}, round {paginationRound}, total items {totalItems}", response?.Pagination?.Cursor, paginationRound, totalItems);
                uri = QueryHelpers.AddQueryString(uri, "query", "*");
                uri = QueryHelpers.AddQueryString(uri, "first", "100");
                if (response?.Pagination?.Cursor != null)
                {
                    uri = QueryHelpers.AddQueryString(uri, "after", response.Pagination.Cursor);
                }
                var result = await _httpClient.GetAsync(uri);
                response = await JsonSerializer.DeserializeAsync<HelixCategoriesSearchResponse>(await result.Content.ReadAsStreamAsync());

                _logger.LogDebug("Received response from Twitch API, items {responseItems}, pagination cursor {paginationCursor}", response?.Data?.Length, response?.Pagination?.Cursor ?? "not set");
                if (response.Data != null)
                {
                    foreach (var category in response.Data)
                    {
                        yield return category;
                    }

                    totalItems += response.Data.Length;
                }

                ++paginationRound;
            } while (response.Pagination.Cursor != null);
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
