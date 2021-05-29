using BlipBloopBot.Storage;
using Conceptoire.Twitch;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Model;
using Conceptoire.Twitch.Steam;
using McMaster.Extensions.CommandLineUtils;
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
    [Command("sync", Description = "Synchronize missing localized entries on remote storage")]
    [HelpOption]
    public class TwitchCategoriesSynchronizationService : IHostedService
    {
        private readonly TwitchAPIClient _twitchAPIClient;
        private readonly IGDBClient _igdbClient;
        private readonly SteamStoreClient _steamStoreClient;
        private readonly TwitchApplicationOptions _options;
        private readonly IGameLocalizationStore _gameLocalization;
        private readonly ILogger _logger;

        [Option("-l", CommandOptionType.SingleValue, Description = "Language to import from Steam")]
        public string TargetLanguage { get; set; }

        [Option("-c", CommandOptionType.SingleOrNoValue, Description = "Cleanup SteamIds found to have no store data associated")]
        public bool CleanupEntries { get; set; }

        public Dictionary<string, GameInfo> _baseEntries;
        public HashSet<GameInfo> _steamEntries;
        public Dictionary<string, GameInfo> _locEntries;

        public TwitchCategoriesSynchronizationService(
            TwitchAPIClient twitchApiClient,
            IGDBClient igdbClient,
            SteamStoreClient steamStoreClient,
            IOptions<TwitchApplicationOptions> options,
            IGameLocalizationStore gameLocalizationStore,
            ILogger<TwitchCategoriesSynchronizationService> logger)
        {
            _twitchAPIClient = twitchApiClient;
            _igdbClient = igdbClient;
            _steamStoreClient = steamStoreClient;
            _steamStoreClient.WebAPIKey = options.Value.SteamApiKey;
            _gameLocalization = gameLocalizationStore;
            _options = options.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await OnExecuteAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            await LoadAllEntries(cancellationToken);
        }

        private async Task LoadAllEntries(CancellationToken cancellationToken)
        {
            _baseEntries = new Dictionary<string, GameInfo>();
            _locEntries = new Dictionary<string, GameInfo>();
            _steamEntries = new HashSet<GameInfo>();

            _logger.LogInformation("Enumerating all generic category entries");
            await foreach(var gameInfo in _gameLocalization.EnumerateGameInfoAsync("", cancellationToken))
            {
                _baseEntries.Add(gameInfo.TwitchCategoryId, gameInfo);
                if (gameInfo.SteamId.HasValue)
                {
                    _steamEntries.Add(gameInfo);
                }
            }

            _logger.LogInformation("Found {totalCount} categories in storage, {steamCount} have an associated Steam id.", _baseEntries.Count, _steamEntries.Count);

            _logger.LogInformation("Enumerating all category entries for locale {targetLanguage}", TargetLanguage);
            await foreach(var gameInfo in _gameLocalization.EnumerateGameInfoAsync(TargetLanguage, cancellationToken))
            {
                _locEntries.Add(gameInfo.TwitchCategoryId, gameInfo);
            }

            _logger.LogInformation("Found {totalCount} categories for locale {targetLanguage} in storage.", _locEntries.Count, TargetLanguage);

            var missingEntryCount = _steamEntries.Where(g => !_locEntries.ContainsKey(g.TwitchCategoryId)).Count();
            _logger.LogInformation("Found {totalCount} missing categories for locale {targetLanguage} in storage.", missingEntryCount, TargetLanguage);

            foreach (var steamGame in _steamEntries)
            {
                if (!_locEntries.ContainsKey(steamGame.TwitchCategoryId))
                {
                    var steamInfo = await _steamStoreClient.GetStoreDetails(steamGame.SteamId.ToString(), TargetLanguage);
                    if (steamInfo != null)
                    {
                        var localizedGameInfo = new GameInfo
                        {
                            TwitchCategoryId = steamGame.TwitchCategoryId,
                            Name = steamGame.Name,
                            IGDBId = steamGame.IGDBId,
                            Summary = steamInfo.ShortDescription,
                            Language = TargetLanguage,
                            Source = "steam",
                            SteamId = steamInfo.SteamAppid
                        };
                        _logger.LogInformation("Saving {locale} loc for game {gameName}(SteamId {steamId}).", TargetLanguage, steamGame.Name, steamInfo.SteamAppid);
                        await _gameLocalization.SaveGameInfoAsync(localizedGameInfo, cancellationToken);
                    }
                    else
                    {
                        steamGame.SteamId = null;
                        
                        if (CleanupEntries)
                        {
                            _logger.LogWarning("No Steam store data found for game {gameName}(SteamId {steamId}), cleaning up the SteamId on master.", steamGame.Name, steamGame.SteamId);
                            await _gameLocalization.SaveGameInfoAsync(steamGame, cancellationToken);
                        }
                        else
                        {
                            _logger.LogWarning("No Steam store data found for game {gameName}(SteamId {steamId}), add -c to clean up the association.", steamGame.Name, steamGame.SteamId);
                        }
                    }
                }
            }
        }
    }
}
