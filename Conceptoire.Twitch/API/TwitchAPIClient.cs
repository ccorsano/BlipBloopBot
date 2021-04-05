using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Conceptoire.Twitch.Constants.TwitchConstants;

namespace Conceptoire.Twitch.API
{
    public class TwitchAPIClient : IDisposable
    {
        private readonly IAuthenticated _authenticated;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public TwitchAPIClient(IAuthenticated authenticated, IHttpClientFactory factory, ILogger<TwitchAPIClient> logger)
            :this(authenticated, factory.CreateClient(), logger) {}

        internal TwitchAPIClient(IAuthenticated authenticated, HttpClient httpClient, ILogger<TwitchAPIClient> logger)
        {
            _authenticated = authenticated;
            _httpClient = httpClient;
            _logger = logger;
        }

        public static TwitchAPIClient CreateFromBase(TwitchAPIClient baseInstance, IAuthenticated authenticated)
        {
            return new TwitchAPIClient(authenticated, baseInstance._httpClient, baseInstance._logger as ILogger<TwitchAPIClient>);
        }

        public async Task<HelixEventSubSubscriptionData> CreateEventSubSubscription(HelixEventSubSubscriptionCreateRequest request, CancellationToken cancellationToken)
        {
            var uri = "https://api.twitch.tv/helix/eventsub/subscriptions";
            var jsonContent = JsonContent.Create(request);
            var jsonMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = jsonContent
            };
            await _authenticated.AuthenticateMessageAsync(jsonMessage, cancellationToken);

            var result = await _httpClient.SendAsync(jsonMessage, cancellationToken);
            var response = await JsonSerializer.DeserializeAsync<HelixEventSubSubscriptionsListReponse>(await result.Content.ReadAsStreamAsync());
            return response.Data[0];
        }

        public async Task<HelixValidateTokenResponse> ValidateToken(CancellationToken cancellationToken = default)
        {
            var uri = "https://id.twitch.tv/oauth2/validate";
            var message = new HttpRequestMessage(HttpMethod.Get, uri);
            await _authenticated.AuthenticateMessageAsync(message, cancellationToken);

            var result = await _httpClient.SendAsync(message, cancellationToken);
            if (!result.IsSuccessStatusCode)
            {
                return null;
            }
            return await JsonSerializer.DeserializeAsync<HelixValidateTokenResponse>(await result.Content.ReadAsStreamAsync());
        }

        public async Task DeleteEventSubSubscription(string subscriptionId, CancellationToken cancellationToken)
        {
            var uri = "https://api.twitch.tv/helix/eventsub/subscriptions";
            uri = QueryHelpers.AddQueryString(uri, "id", subscriptionId);

            var deleteMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
            await _authenticated.AuthenticateMessageAsync(deleteMessage, cancellationToken);

            var result = await _httpClient.SendAsync(deleteMessage, cancellationToken);

            result.EnsureSuccessStatusCode();
        }

        public IAsyncEnumerable<HelixEventSubSubscriptionData> EnumerateEventSubSubscriptions(EventSubStatus? status = null)
        {
            var uri = "https://api.twitch.tv/helix/eventsub/subscriptions";
            if (status.HasValue)
            {
                uri = QueryHelpers.AddQueryString(uri, "status", GetEventSubStatusString(status.Value));
            }
            return EnumerateHelixAPIAsync<HelixEventSubSubscriptionsListReponse, HelixEventSubSubscriptionData>(uri);
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
                var response = await JsonSerializer.DeserializeAsync<HelixChannelGetInfoResponse>(await result.Content.ReadAsStreamAsync());
                return response.Data?[0] ?? null;
            }
            else
            {
                var response = await result.Content.ReadAsStringAsync();
                _logger.LogError(response);
            }

            return null;
        }

        public IAsyncEnumerable<HelixCategoriesSearchEntry> EnumerateTwitchCategoriesAsync()
        {
            var baseUri = "https://api.twitch.tv/helix/search/categories";
            baseUri = QueryHelpers.AddQueryString(baseUri, "query", "*");
            return EnumerateHelixAPIAsync<HelixCategoriesSearchEntry>(baseUri);
        }

        public async Task<HelixChannelEditor[]> GetHelixChannelEditorsAsync(string broadcasterId)
        {
            var httpMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.twitch.tv/helix/channels/editors?broadcaster_id={broadcasterId}")
            };
            await _authenticated.AuthenticateMessageAsync(httpMessage);
            var result = await _httpClient.SendAsync(httpMessage);
            if (result.IsSuccessStatusCode)
            {
                var response = await JsonSerializer.DeserializeAsync<HelixChannelGetEditorsResponse>(await result.Content.ReadAsStreamAsync());
                return response.Data;
            }

            var errorMessage = await result.Content.ReadAsStringAsync();
            _logger.LogError(errorMessage);
            result.EnsureSuccessStatusCode(); // throws
            throw new Exception("Unexpected error: Unreachable exception.");
        }

        public IAsyncEnumerable<HelixChannelModerator> EnumerateChannelModeratorsAsync(string broadcasterId)
        {
            var baseUri = "https://api.twitch.tv/helix/moderation/moderators";
            baseUri = QueryHelpers.AddQueryString(baseUri, "broadcaster_id", broadcasterId);
            return EnumerateHelixAPIAsync<HelixChannelModerator>(baseUri);
        }

        private IAsyncEnumerable<TEntry> EnumerateHelixAPIAsync<TEntry>(string baseUri)
            where TEntry:class
            => EnumerateHelixAPIAsync<HelixPaginatedResponse<TEntry>, TEntry>(baseUri);

        /// <summary>
        /// Internal generic method to enumerate asynchronously through Twitch Helix responses
        /// </summary>
        /// <remarks>
        /// TODO: Support communicating extra fields in enumeration response to caller
        /// </remarks>
        /// <typeparam name="TResponse">Enumeration response type</typeparam>
        /// <typeparam name="TEntry">Enumeration entry type</typeparam>
        /// <param name="baseUri">Formatted GET Uri, with base arguments in query string</param>
        /// <returns>Async enumeration through the HTTP Get response</returns>
        private async IAsyncEnumerable<TEntry> EnumerateHelixAPIAsync<TResponse, TEntry>(string baseUri)
            where TEntry:class
            where TResponse: HelixPaginatedResponse<TEntry>
        {
            HelixPaginatedResponse<TEntry> response = null;
            uint paginationRound = 1;
            int totalItems = 0;
            do
            {
                var uri = baseUri;
                _logger.LogDebug("Enumerating from Twitch API on URI {baseUri}, pagination {paginationCursor}, round {paginationRound}, total items {totalItems}", baseUri, response?.Pagination?.Cursor, paginationRound, totalItems);
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
                response = await JsonSerializer.DeserializeAsync<HelixPaginatedResponse<TEntry>>(await result.Content.ReadAsStreamAsync());

                _logger.LogDebug("Received response from Twitch API, items {responseItems}, pagination cursor {paginationCursor}", response?.Data?.Length, response?.Pagination?.Cursor ?? "not set");
                if (response.Data != null)
                {
                    foreach (var entry in response.Data)
                    {
                        yield return entry;
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
