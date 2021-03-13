using BlipBloopBot.Twitch;
using BlipBloopBot.Twitch.API;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchCategoriesCrawler
{
    public class TwitchCategoriesCrawlerService : IHostedService
    {
        private readonly TwitchAPIClient _twitchAPIClient;
        private readonly IGDBClient _igdbClient;
        private readonly TwitchApplicationOptions _options;
        private readonly ILogger _logger;

        public TwitchCategoriesCrawlerService(TwitchAPIClient twitchApiClient, IGDBClient igdbClient, IOptions<TwitchApplicationOptions> options, ILogger<TwitchCategoriesCrawlerService> logger)
        {
            _twitchAPIClient = twitchApiClient;
            _igdbClient = igdbClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _twitchAPIClient.AuthenticateAsync(_options.ClientId, _options.ClientSecret);
            await _igdbClient.AuthenticateAsync(_options.ClientId, _options.ClientSecret);
            // Scan all categories
            await foreach(var category in _twitchAPIClient.EnumerateTwitchCategoriesAsync())
            {
                _logger.LogInformation("Category: {categoryName}, id: {categoryId}", category.Name, category.Id);
                var igdbExternalEntry = await _igdbClient.SearchExternalGame("uid", $"\"{category.Id}\"", IGDBExternalGameCategory.Twitch);
                if (igdbExternalEntry.Length == 0)
                {
                    _logger.LogInformation("No IGDB entry");
                    continue;
                }

                _logger.LogInformation("Found IGDB entry: {igdbName}, {igdbGameId}", igdbExternalEntry.First().Name, igdbExternalEntry.First().Game);

                var igdbEntry = await _igdbClient.GetGameByIdAsync(igdbExternalEntry.First().Game);
                if (igdbEntry == null)
                {
                    _logger.LogInformation("Failed to resolve IGDB entry");
                    continue;
                }

                _logger.LogInformation("Resolved IGDB entry: {igdbName}, {igdbGameId}", igdbEntry.Platforms, igdbExternalEntry.First().Game);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
