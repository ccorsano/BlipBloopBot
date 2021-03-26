using BlipBloopBot.Twitch.Authentication;
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
        private readonly IAuthenticated _authenticated;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public TwitchAPIClient(IAuthenticated authenticated, IHttpClientFactory factory, ILogger<TwitchAPIClient> logger)
        {
            _authenticated = authenticated;
            _httpClient = factory.CreateClient();
            _logger = logger;
        }

        public async Task<HelixChannelSearchResult[]> SearchChannelsAsync(string channelQuery)
        {
            var httpMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.twitch.tv/helix/search/channels?query={channelQuery}")
            };
            await _authenticated.AuthenticateMessageAsync(httpMessage);

            var result = await _httpClient.SendAsync(httpMessage);
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
            var httpMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.twitch.tv/helix/channels?broadcaster_id={broadcasterId}")
            };
            await _authenticated.AuthenticateMessageAsync(httpMessage);
            var result = await _httpClient.SendAsync(httpMessage);
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
                var httpMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(uri)
                };
                await _authenticated.AuthenticateMessageAsync(httpMessage);
                var result = await _httpClient.SendAsync(httpMessage);
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
