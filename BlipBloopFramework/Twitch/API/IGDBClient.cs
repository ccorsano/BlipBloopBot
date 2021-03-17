using BlibBloopBot.IGDB.Generated;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.API
{
    public enum IGDBExternalGameCategory
    {
        Steam = 1,
        Gog = 5,
        YouTube = 10,
        Microsoft = 11,
        Apple = 12,
        Twitch = 14,
        Android = 15,
    }

    public class IGDBClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly TwitchAPIClient _twitchAPIClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;
        private string _clientId;
        private string _clientSecret;

        public IGDBClient(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, TwitchAPIClient twitchAPIClient, ILogger<IGDBClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://api.igdb.com/");
            _twitchAPIClient = twitchAPIClient;
            _cache = memoryCache;
            _logger = logger;
        }

        public async Task AuthenticateAsync(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            await _twitchAPIClient.AuthenticateAsync(clientId, clientSecret);
        }

        public async Task<Game> GetGameByIdAsync(ulong gameId)
        {
            var cacheKey = $"igdb:games:{gameId}";

            if (!_cache.TryGetValue(cacheKey, out Game result))
            {
                var authToken = await _twitchAPIClient.AuthenticateAsync();

                if (authToken == null)
                {
                    throw new InvalidOperationException("Please authenticate first");
                }

                var message = new HttpRequestMessage(HttpMethod.Post, $"v4/games.pb");
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                message.Headers.Add("Client-ID", _clientId);
                message.Content = new StringContent($"fields *; where id = {gameId};");

                var response = await _httpClient.SendAsync(message);
                response.EnsureSuccessStatusCode();

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    result = Serializer.Deserialize<GameResult>(responseStream).Games.FirstOrDefault();
                }

                _cache.Set(cacheKey, result);
            }
            return result;
        }

        public async Task<ExternalGame[]> SearchExternalGame(string fieldName, string fieldValue, IGDBExternalGameCategory category)
        {
            var cacheKey = $"igdb:external:{fieldName}:{fieldValue}:{category}";

            if (!_cache.TryGetValue(cacheKey, out ExternalGame[] result))
            {
                var authToken = await _twitchAPIClient.AuthenticateAsync();

                if (authToken == null)
                {
                    throw new InvalidOperationException("Please authenticate first");
                }

                var message = new HttpRequestMessage(HttpMethod.Post, $"v4/external_games.pb");
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                message.Headers.Add("Client-ID", _clientId);
                message.Content = new StringContent($"fields *; where {fieldName} = {fieldValue} & category = {(uint) category};");

                var response = await _httpClient.SendAsync(message);
                response.EnsureSuccessStatusCode();

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    result = Serializer.Deserialize<ExternalGameResult>(responseStream).Externalgames.ToArray();
                }

                _cache.Set(cacheKey, result);
            }
            return result;
        }

        public async IAsyncEnumerable<Platform> EnumeratePlatforms()
        {
            var authToken = await _twitchAPIClient.AuthenticateAsync();

            List<Platform> response = null;
            uint page = 0;
            int totalItems = 0;
            const int limit = 100;
            do
            {
                var message = new HttpRequestMessage(HttpMethod.Post, $"v4/platforms.pb");
                _logger.LogDebug("Fetching platforms from IGDB API, {offset}-{nextOffset}, total items {totalItems}", (page * limit) + 1, (page+1) * limit, totalItems);

                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                message.Headers.Add("Client-ID", _clientId);
                message.Content = new StringContent($"fields *; limit {limit}; offset {page * limit};");

                var result = await _httpClient.SendAsync(message);
                var platformResult = Serializer.Deserialize<PlatformResult>(await result.Content.ReadAsStreamAsync());
                response = platformResult.Platforms;

                _logger.LogDebug("Received response from IGDB API, items {responseItems}", response.Count);
                
                if (response != null)
                {
                    foreach (var platform in response)
                    {
                        yield return platform;
                    }

                    totalItems += response.Count;
                }

                ++page;
            } while (response.Count == limit);
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
