﻿using Conceptoire.Twitch.Constants;
using Conceptoire.Twitch.Steam.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.Steam
{
    public class SteamStoreClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;

        private readonly IAsyncPolicy<HttpResponseMessage> RetryPolicy;

        public SteamStoreClient(IHttpClientFactory factory, IMemoryCache memoryCache, ILogger<SteamStoreClient> logger)
        {
            _httpClient = factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://store.steampowered.com/");
            _cache = memoryCache;
            _logger = logger;

            RetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.BadGateway)
                .WaitAndRetryAsync(
                    retryCount: 5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(5, retryAttempt)),
                    (exception, timeSpan) =>
                    {
                        _logger.LogError("Call to steam throttled, executing exponential backoff {backoff}", timeSpan);
                    });
        }

        public string WebAPIKey { get; set; }

        public async Task<SteamStoreDetails> GetStoreDetails(string appId, string language = "en-US")
        {
            var apiLanguageCode = SteamConstants.SteamLanguageCodeToAPILanguageCode[language];

            var cacheKey = $"steam:store:{appId}:{apiLanguageCode}";


            if (!_cache.TryGetValue(cacheKey, out SteamStoreDetails result))
            {
                var response = await RetryPolicy.ExecuteAsync(() =>
                {
                    var message = new HttpRequestMessage(HttpMethod.Get, $"api/appdetails/?appids={appId}&l={apiLanguageCode}");
                    message.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                    //message.Headers.CacheControl.NoCache = true;
                    if (!string.IsNullOrEmpty(WebAPIKey))
                    {
                        message.Headers.Add("x-webapi-key", WebAPIKey);
                    }
                    return _httpClient.SendAsync(message);
                });
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
