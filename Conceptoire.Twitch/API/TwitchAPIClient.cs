using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using static Conceptoire.Twitch.Constants.TwitchConstants;

namespace Conceptoire.Twitch.API
{
    public class TwitchAPIClient : IDisposable, ITwitchAPIClient
    {
        private readonly IAuthenticated _authenticated;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public TwitchAPIClient(IAuthenticated authenticated, IHttpClientFactory factory, ILogger<TwitchAPIClient> logger)
            : this(authenticated, factory.CreateClient(), logger) { }

        internal TwitchAPIClient(IAuthenticated authenticated, HttpClient httpClient, ILogger<TwitchAPIClient> logger)
        {
            _authenticated = authenticated;
            _httpClient = httpClient;
            _logger = logger;
        }

        public static TwitchAPIClient Create(IAuthenticated authenticated)
        {
            var loggerFactory = new LoggerFactory();
            var httpClient = new HttpClient();
            return new TwitchAPIClient(authenticated, httpClient, loggerFactory.CreateLogger<TwitchAPIClient>());
        }

        public static TwitchAPIClient CreateFromBase(TwitchAPIClient baseInstance, IAuthenticated authenticated)
        {
            return new TwitchAPIClient(authenticated, baseInstance._httpClient, baseInstance._logger as ILogger<TwitchAPIClient>);
        }

        public async Task<HelixEventSubSubscriptionData> CreateEventSubSubscription(HelixEventSubSubscriptionCreateRequest request, CancellationToken cancellationToken = default)
        {
            var uri = "https://api.twitch.tv/helix/eventsub/subscriptions";
            var jsonContent = JsonContent.Create(request, HelixJsonContext.Default.HelixEventSubSubscriptionCreateRequest);
            jsonContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var jsonMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = jsonContent
            };
            await _authenticated.AuthenticateMessageAsync(jsonMessage, cancellationToken);

            var result = await _httpClient.SendAsync(jsonMessage, cancellationToken);
            result.EnsureSuccessStatusCode();

            var response = await JsonSerializer.DeserializeAsync(await result.Content.ReadAsStreamAsync(), HelixJsonContext.Default.HelixEventSubSubscriptionsListReponse);
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
            return await JsonSerializer.DeserializeAsync<HelixValidateTokenResponse>(await result.Content.ReadAsStreamAsync(), HelixJsonContext.Default.HelixValidateTokenResponse);
        }

        public async Task DeleteEventSubSubscription(string subscriptionId, CancellationToken cancellationToken = default)
        {
            var uri = "https://api.twitch.tv/helix/eventsub/subscriptions";
            uri = QueryHelpers.AddQueryString(uri, "id", subscriptionId);

            var deleteMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
            await _authenticated.AuthenticateMessageAsync(deleteMessage, cancellationToken);

            var result = await _httpClient.SendAsync(deleteMessage, cancellationToken);

            result.EnsureSuccessStatusCode();
        }

        public IAsyncEnumerable<HelixEventSubSubscriptionData> EnumerateEventSubSubscriptions(EventSubStatus? status = null, CancellationToken cancellationToken = default)
        {
            var uri = "https://api.twitch.tv/helix/eventsub/subscriptions";
            if (status.HasValue)
            {
                uri = QueryHelpers.AddQueryString(uri, "status", GetEventSubStatusString(status.Value));
            }
            return EnumerateHelixAPIAsync<HelixEventSubSubscriptionsListReponse, HelixEventSubSubscriptionData>(uri, HelixJsonContext.Default.HelixEventSubSubscriptionsListReponse, cancellationToken);
        }

        public Task<HelixUsersGetResult[]> GetUsersByLoginAsync(IEnumerable<string> logins, CancellationToken cancellationToken = default)
            => GetUsersAsync(new string[0], logins, cancellationToken);

        public Task<HelixUsersGetResult[]> GetUsersByIdAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
            => GetUsersAsync(ids, new string[0], cancellationToken);

        public async Task<HelixUsersGetResult[]> GetUsersAsync(IEnumerable<string> ids, IEnumerable<string> logins, CancellationToken cancellationToken = default)
        {
            var uri = "https://api.twitch.tv/helix/users";
            foreach (var id in ids)
            {
                uri = QueryHelpers.AddQueryString(uri, "id", id);
            }
            foreach (var login in logins)
            {
                uri = QueryHelpers.AddQueryString(uri, "login", login);
            }
            var httpMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri)
            };
            await _authenticated.AuthenticateMessageAsync(httpMessage, cancellationToken);

            var result = await _httpClient.SendAsync(httpMessage, cancellationToken);
            if (result.IsSuccessStatusCode)
            {
                var response = await JsonSerializer.DeserializeAsync(await result.Content.ReadAsStreamAsync(), HelixUsersGetResponseJsonContext.Default.HelixUsersGetResponse, cancellationToken);
                return response.Data;
            }
            else
            {
                var response = await result.Content.ReadAsStringAsync();
                _logger.LogError(response);
            }

            return null;
        }

        public async Task<HelixChannelSearchResult[]> SearchChannelsAsync(string channelQuery, CancellationToken cancellationToken = default)
        {
            var httpMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.twitch.tv/helix/search/channels?query={channelQuery}")
            };
            await _authenticated.AuthenticateMessageAsync(httpMessage, cancellationToken);

            var result = await _httpClient.SendAsync(httpMessage, cancellationToken);
            if (result.IsSuccessStatusCode)
            {
                var response = await JsonSerializer.DeserializeAsync(await result.Content.ReadAsStreamAsync(), HelixChannelsSearchResponseContext.Default.HelixChannelsSearchResponse, cancellationToken);
                return response.Data;
            }
            else
            {
                var response = await result.Content.ReadAsStringAsync();
                _logger.LogError(response);
            }

            return null;
        }

        public async Task<HelixChannelInfo> GetChannelInfoAsync(string broadcasterId, CancellationToken cancellationToken = default)
        {
            var httpMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.twitch.tv/helix/channels?broadcaster_id={broadcasterId}")
            };
            await _authenticated.AuthenticateMessageAsync(httpMessage, cancellationToken);
            var result = await _httpClient.SendAsync(httpMessage, cancellationToken);
            if (result.IsSuccessStatusCode)
            {
                var response = await JsonSerializer.DeserializeAsync(await result.Content.ReadAsStreamAsync(), HelixChannelGetInfoResponseContext.Default.HelixChannelGetInfoResponse, cancellationToken);
                return response.Data?[0] ?? null;
            }
            else
            {
                var response = await result.Content.ReadAsStringAsync();
                _logger.LogError(response);
            }

            return null;
        }

        public IAsyncEnumerable<HelixGetStreamsEntry> EnumerateStreamsAsync(CancellationToken cancellationToken = default)
        {
            var baseUri = "https://api.twitch.tv/helix/streams";
            return EnumerateHelixAPIAsync(baseUri, HelixJsonContext.Default.HelixPaginatedResponseHelixGetStreamsEntry, cancellationToken);
        }

        public IAsyncEnumerable<HelixCategoriesSearchEntry> EnumerateTwitchCategoriesAsync(CancellationToken cancellationToken = default)
            => EnumerateTwitchCategoriesAsync("*", cancellationToken);

        public IAsyncEnumerable<HelixCategoriesSearchEntry> EnumerateTwitchCategoriesAsync(string query, CancellationToken cancellationToken = default)
        {
            var baseUri = "https://api.twitch.tv/helix/search/categories";
            baseUri = QueryHelpers.AddQueryString(baseUri, "query", query);
            return EnumerateHelixAPIAsync(baseUri, HelixJsonContext.Default.HelixPaginatedResponseHelixCategoriesSearchEntry, cancellationToken);
        }

        public IAsyncEnumerable<HelixVideoInfo> EnumerateTwitchChannelVideosAsync(string channelId, string videoType = null, CancellationToken cancellationToken = default)
        {
            var baseUri = "https://api.twitch.tv/helix/videos";
            baseUri = QueryHelpers.AddQueryString(baseUri, "user_id", channelId);
            if (!string.IsNullOrEmpty(videoType))
            {
                baseUri = QueryHelpers.AddQueryString(baseUri, "type", videoType);
            }
            return EnumerateHelixAPIAsync(baseUri, HelixJsonContext.Default.HelixPaginatedResponseHelixVideoInfo, cancellationToken);
        }

        public IAsyncEnumerable<HelixClip> EnumerateTwitchChannelClipsAsync(string channelId, DateTime? startedAt = null, DateTime? endedAt = null, CancellationToken cancellationToken = default)
        {
            var baseUri = "https://api.twitch.tv/helix/clips";
            baseUri = QueryHelpers.AddQueryString(baseUri, "broadcaster_id", channelId);
            if (startedAt != null)
            {
                baseUri = $"{baseUri}&started_at={startedAt.Value.ToString("yyyy-MM-dd'T'HH:mm:ssZ")}";
            }
            if (endedAt != null)
            {
                baseUri = $"{baseUri}&ended_at={endedAt.Value.ToString("yyyy-MM-dd'T'HH:mm:ssZ")}";
            }
            return EnumerateHelixAPIAsync(baseUri, HelixJsonContext.Default.HelixPaginatedResponseHelixClip, cancellationToken);
        }

        public IAsyncEnumerable<HelixCategoriesSearchEntry> EnumerateTopGamesAsync(CancellationToken cancellationToken = default)
        {
            var baseUri = "https://api.twitch.tv/helix/games/top";
            return EnumerateHelixAPIAsync(baseUri, HelixJsonContext.Default.HelixPaginatedResponseHelixCategoriesSearchEntry, cancellationToken);
        }

        public async Task<HelixCategoriesSearchEntry[]> GetGamesInfo(string[] categoryIds, CancellationToken cancellationToken = default)
        {
            var uri = "https://api.twitch.tv/helix/games";
            foreach (var id in categoryIds)
            {
                uri = QueryHelpers.AddQueryString(uri, "id", id);
            }
            var httpMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri)
            };
            await _authenticated.AuthenticateMessageAsync(httpMessage, cancellationToken);
            var result = await _httpClient.SendAsync(httpMessage, cancellationToken);
            if (result.IsSuccessStatusCode)
            {
                var response = await JsonSerializer.DeserializeAsync(await result.Content.ReadAsStreamAsync(), HelixGamesResponseContext.Default.HelixGamesResponse, cancellationToken);
                return response.Data;
            }

            var errorMessage = await result.Content.ReadAsStringAsync();
            _logger.LogError(errorMessage);
            result.EnsureSuccessStatusCode(); // throws
            throw new Exception("Unexpected error: Unreachable exception.");
        }

        public async Task<HelixChannelEditor[]> GetHelixChannelEditorsAsync(string broadcasterId, CancellationToken cancellationToken = default)
        {
            var httpMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.twitch.tv/helix/channels/editors?broadcaster_id={broadcasterId}")
            };
            await _authenticated.AuthenticateMessageAsync(httpMessage, cancellationToken);
            var result = await _httpClient.SendAsync(httpMessage, cancellationToken);
            if (result.IsSuccessStatusCode)
            {
                var response = await JsonSerializer.DeserializeAsync(await result.Content.ReadAsStreamAsync(), HelixChannelGetEditorsResponseContext.Default.HelixChannelGetEditorsResponse, cancellationToken);
                return response.Data;
            }

            var errorMessage = await result.Content.ReadAsStringAsync();
            _logger.LogError(errorMessage);
            result.EnsureSuccessStatusCode(); // throws
            throw new Exception("Unexpected error: Unreachable exception.");
        }

        public IAsyncEnumerable<HelixChannelModerator> EnumerateChannelModeratorsAsync(string broadcasterId, CancellationToken cancellationToken = default)
        {
            var baseUri = "https://api.twitch.tv/helix/moderation/moderators";
            baseUri = QueryHelpers.AddQueryString(baseUri, "broadcaster_id", broadcasterId);
            return EnumerateHelixAPIAsync(baseUri, HelixJsonContext.Default.HelixPaginatedResponseHelixChannelModerator, cancellationToken);
        }

        public IAsyncEnumerable<HelixExtensionLiveChannel> EnumerateExtensionLiveChannelsAsync(string extensionId, CancellationToken cancellationToken = default)
        {
            var baseUri = "https://api.twitch.tv/helix/extensions/live";
            baseUri = QueryHelpers.AddQueryString(baseUri, "extension_id", extensionId);
            return EnumerateHelixAPIAsync(baseUri, HelixJsonContext.Default.HelixPaginatedResponseHelixExtensionLiveChannel, cancellationToken);
        }

        private IAsyncEnumerable<TEntry> EnumerateHelixAPIAsync<TEntry>(string baseUri, JsonTypeInfo<HelixPaginatedResponse<TEntry>> typeInfo, CancellationToken cancellationToken = default)
            where TEntry : class
            => EnumerateHelixAPIAsync<HelixPaginatedResponse<TEntry>, TEntry>(baseUri, typeInfo, cancellationToken);

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
        private async IAsyncEnumerable<TEntry> EnumerateHelixAPIAsync<TResponse, TEntry>(string baseUri, JsonTypeInfo<TResponse> typeInfo, [EnumeratorCancellation] CancellationToken cancellationToken = default)
            where TEntry : class
            where TResponse : HelixPaginatedResponse<TEntry>
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
                await _authenticated.AuthenticateMessageAsync(httpMessage, cancellationToken);
                var result = await _httpClient.SendAsync(httpMessage, cancellationToken);
                response = await JsonSerializer.DeserializeAsync(await result.Content.ReadAsStreamAsync(), typeInfo, cancellationToken);

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
            } while (! string.IsNullOrEmpty(response?.Pagination?.Cursor));
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
                    { "broadcaster_user_id", broadcasterId }
                },
                Transport = new HelixEventSubTransport
                {
                    Method = "webhook",
                    Callback = callback.ToString(),
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
