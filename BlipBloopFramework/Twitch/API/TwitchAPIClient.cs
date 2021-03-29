using BlipBloopBot.Twitch.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static BlipBloopBot.Constants.TwitchConstants;

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

        public Task AuthenticateAsync(string clientId, string clientSecret) => AuthenticateAsync(clientId, clientSecret, CancellationToken.None);

        public Task<string> AuthenticateAsync() => AuthenticateAsync(CancellationToken.None);

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

        // TODO: this is going to be a large region, take it out when refactoring the API
        #region EventSub

        public Task CreateEventSubChannelUpdateSubscription(string broadcasterId, Uri callback, string secret, CancellationToken? cancellationToken = null)
        {
            var request = new HelixEventSubSubscriptionCreateRequest
            {
                Type = EventSubTypes.ChannelUpdate,
                Condition = new Dictionary<string, string>
                {
                    { "broadcaster_id", broadcasterId }
                },
                Transport = new HelixEventSubTransport
                {
                    Method = "webhook",
                    Callback = callback,
                    Secret = secret,
                },
                Version = "1",
            };

            return CreateEventSubSubscription(request, cancellationToken ?? CancellationToken.None);
        }

        public async Task<HelixEventSubSubscriptionData> CreateEventSubSubscription(HelixEventSubSubscriptionCreateRequest request, CancellationToken cancellationToken)
        {
            await AuthenticateAsync();

            var uri = "https://api.twitch.tv/helix/eventsub/subscriptions";
            var result = await _httpClient.PostAsJsonAsync(uri, request, cancellationToken);
            var response = await JsonSerializer.DeserializeAsync<HelixEventSubSubscriptionsListReponse>(await result.Content.ReadAsStreamAsync());
            return response.Data[0];
        }

        public async Task DeleteEventSubSubscription(string subscriptionId, CancellationToken cancellationToken)
        {
            await AuthenticateAsync();

            var uri = "https://api.twitch.tv/helix/eventsub/subscriptions";
            uri = QueryHelpers.AddQueryString(uri, "id", subscriptionId);
            var result = await _httpClient.DeleteAsync(uri, cancellationToken);

            result.EnsureSuccessStatusCode();
        }

        public async IAsyncEnumerable<HelixEventSubSubscriptionData> EnumerateEventSubSubscriptions(EventSubStatus? status = null)
        {
            await AuthenticateAsync();

            HelixEventSubSubscriptionsListReponse response = null;
            uint paginationRound = 1;
            int totalItems = 0;
            do
            {
                var uri = "https://api.twitch.tv/helix/eventsub/subscriptions";
                _logger.LogDebug("Fetching EventSub subscriptions from Twitch API, pagination {paginationCursor}, round {paginationRound}, total items {totalItems}", response?.Pagination?.Cursor, paginationRound, totalItems);
                if (status.HasValue)
                {
                    uri = QueryHelpers.AddQueryString(uri, "status", GetEventSubStatusString(status.Value));
                }
                if (response?.Pagination?.Cursor != null)
                {
                    uri = QueryHelpers.AddQueryString(uri, "after", response.Pagination.Cursor);
                }
                var result = await _httpClient.GetAsync(uri);
                response = await JsonSerializer.DeserializeAsync<HelixEventSubSubscriptionsListReponse>(await result.Content.ReadAsStreamAsync());

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


        #endregion

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }
    }
}
