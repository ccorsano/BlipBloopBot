using BlipBloopBot.Steam.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.API
{
    public class SteamStoreClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;

        public SteamStoreClient(IHttpClientFactory factory, IMemoryCache memoryCache, ILogger<SteamStoreClient> logger)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://store.steampowered.com/");
            _logger = logger;
        }

        public async Task<SteamStoreDetails> GetStoreDetails(string appId, string language = "en-US")
        {
            var cacheKey = $"steam:store:{appId}:{language}";

            if (!_cache.TryGetValue(cacheKey, out SteamStoreDetails result))
            {
                var message = new HttpRequestMessage(HttpMethod.Get, $"api/appdetails/?appids={appId}");
                message.Headers.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue(language));
                var response = await _httpClient.SendAsync(message);
                response.EnsureSuccessStatusCode();

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    var wrapper = await JsonSerializer.DeserializeAsync<SteamStoreResult>(responseStream, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    result = wrapper[appId].Data;
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
