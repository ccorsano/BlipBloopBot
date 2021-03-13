using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
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

        public async Task<IGDBGame> GetGameByIdAsync(uint gameId)
        {
            var cacheKey = $"igdb:games:{gameId}";

            if (!_cache.TryGetValue(cacheKey, out IGDBGame result))
            {
                var authToken = await _twitchAPIClient.AuthenticateAsync();

                if (authToken == null)
                {
                    throw new InvalidOperationException("Please authenticate first");
                }

                var message = new HttpRequestMessage(HttpMethod.Post, $"v4/games");
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                message.Headers.Add("Client-ID", _clientId);
                message.Content = new StringContent($"fields *; where id = {gameId};");

                var response = await _httpClient.SendAsync(message);
                response.EnsureSuccessStatusCode();

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    result = (await JsonSerializer.DeserializeAsync<IGDBGame[]>(responseStream, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })).First();
                }

                _cache.Set(cacheKey, result);
            }
            return result;
        }

        public async Task<IGDBExternalGame[]> SearchExternalGame(string fieldName, string fieldValue, IGDBExternalGameCategory category)
        {
            var cacheKey = $"igdb:external:{fieldName}:{fieldValue}:{category}";

            if (!_cache.TryGetValue(cacheKey, out IGDBExternalGame[] result))
            {
                var authToken = await _twitchAPIClient.AuthenticateAsync();

                if (authToken == null)
                {
                    throw new InvalidOperationException("Please authenticate first");
                }

                var message = new HttpRequestMessage(HttpMethod.Post, $"v4/external_games");
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
                message.Headers.Add("Client-ID", _clientId);
                message.Content = new StringContent($"fields *; where {fieldName} = {fieldValue} & category = {(uint) category};");

                var response = await _httpClient.SendAsync(message);
                response.EnsureSuccessStatusCode();

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    result = await JsonSerializer.DeserializeAsync<IGDBExternalGame[]>(responseStream, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _cache.Set(cacheKey, result);
            }
            return result;
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
