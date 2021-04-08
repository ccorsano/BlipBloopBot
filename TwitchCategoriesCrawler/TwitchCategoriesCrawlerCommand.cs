using Conceptoire.Twitch.Constants;
using Conceptoire.Twitch.Model;
using Conceptoire.Twitch.Steam;
using Conceptoire.Twitch;
using Conceptoire.Twitch.API;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conceptoire.Twitch.IGDB.Generated;
using McMaster.Extensions.CommandLineUtils;
using System;

namespace TwitchCategoriesCrawler
{
    [Command(Description = "Crawl Twitch categories and resolve localization from Steam")]
    [HelpOption]
    public class TwitchCategoriesCrawlerCommand
    {
        private readonly TwitchAPIClient _twitchAPIClient;
        private readonly IGDBClient _igdbClient;
        private readonly SteamStoreClient _steamStoreClient;
        private readonly TwitchApplicationOptions _options;
        private readonly ILogger _logger;

        [Option("-l", CommandOptionType.SingleValue, Description = "Language to import from Steam")]
        public string TargetLanguage { get; set; }

        public TwitchCategoriesCrawlerCommand(
            TwitchAPIClient twitchApiClient,
            IGDBClient igdbClient,
            SteamStoreClient steamStoreClient,
            IOptions<TwitchApplicationOptions> options,
            ILogger<TwitchCategoriesCrawlerCommand> logger)
        {
            _twitchAPIClient = twitchApiClient;
            _igdbClient = igdbClient;
            _steamStoreClient = steamStoreClient;
            _steamStoreClient.WebAPIKey = options.Value.SteamApiKey;
            _options = options.Value;
            _logger = logger;
        }


        public async Task OnExecute()
        {
            if (! SteamConstants.TwitchLanguageMapping.ContainsKey(TargetLanguage))
            {
                _logger.LogError("Unrecognised language, should be one of the following: {languages}", string.Join(",", SteamConstants.TwitchLanguageMapping.Keys));
                throw new ArgumentException($"Unrecognised language, should be one of the following: {string.Join(",", SteamConstants.TwitchLanguageMapping.Keys)}");
            }

            IDictionary<ulong, Platform> platformDb = new Dictionary<ulong, Platform>();
            IDictionary<(string, string), GameInfo> gameDb = new Dictionary<(string, string), GameInfo>();

            // Load IGDB platforms
            //await foreach(var platform in _igdbClient.EnumeratePlatforms())
            //{
            //    platformDb.Add(platform.Id, platform);
            //    _logger.LogInformation("Platform {platformName} - {platformCategory} - {platformFamily}", platform.Name, platform.Category, platform.PlatformFamily?.Name);
            //}

            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
            };

            var categoriesList = new List<Task<GameInfo[]>>();
            if (File.Exists("gamedb.csv"))
            {
                using (var textReader = new StreamReader("gamedb.csv"))
                using (var csvReader = new CsvReader(textReader, configuration))
                {
                    foreach (var gameRecord in csvReader.GetRecords<GameInfo>())
                    {
                        if (gameDb.ContainsKey((gameRecord.TwitchCategoryId, gameRecord.Language)))
                        {
                            _logger.LogError("Duplicate entry, skipping {categoryId}, {categoryName}, {language}", gameRecord.TwitchCategoryId, gameRecord.Name, gameRecord.Language);
                            continue;
                        }
                        gameDb.Add((gameRecord.TwitchCategoryId, gameRecord.Language), gameRecord);
                        _logger.LogWarning("Found existing entry {categoryId}, {categoryName}, {language}", gameRecord.TwitchCategoryId, gameRecord.Name, gameRecord.Language);
                    }
                }

                // Avoid writing a bogus header record in the middle of the existing doc
                configuration.HasHeaderRecord = false;
            }

            using (var textWriter = new StreamWriter("gamedb.csv", true))
            using (var csvWriter = new CsvWriter(textWriter, configuration))
            {
                var steamLanguage = SteamConstants.TwitchLanguageMapping.First(tlm => tlm.Key == TargetLanguage);
                //foreach (var steamLanguage in SteamConstants.TwitchLanguageMapping)
                {
                    // Scan all categories
                    await foreach (var category in _twitchAPIClient.EnumerateTwitchCategoriesAsync())
                    {
                        GameInfo gameInfo;

                        if (gameDb.TryGetValue((category.Id, steamLanguage.Key), out gameInfo))
                        {
                            _logger.LogWarning("Found existing entry {categoryId}, {categoryName}, {language}", category.Id, category.Name, steamLanguage.Key);
                            continue;
                        }

                        if (gameDb.TryGetValue((category.Id, TwitchConstants.LanguageCodes.ENGLISH), out gameInfo))
                        {
                            _logger.LogWarning("Found existing base EN entry for {categoryId}, {categoryName}", category.Id, category.Name);
                        }
                        else
                        {
                            if (steamLanguage.Key != TwitchConstants.LanguageCodes.ENGLISH && !gameDb.TryAdd((category.Id, TwitchConstants.LanguageCodes.ENGLISH), gameInfo))
                            {
                                _logger.LogWarning("Received same category twice {categoryId} ({categoryName})", category.Id, category.Name);
                            }
                        }

                        if (gameInfo == null)
                        {
                            gameInfo = new GameInfo
                            {
                                TwitchCategoryId = category.Id,
                                Name = category.Name,
                            };
                        }

                        // If we already resolved that category for this language, skip it
                        if (gameDb.ContainsKey((category.Id, steamLanguage.Key)))
                        {
                            _logger.LogWarning("Found existing localized {language} entry for {categoryId}, {categoryName}", steamLanguage.Key, category.Id, category.Name);
                            continue;
                        }

                        csvWriter.WriteRecords(await FetchCategoryInfo(category, steamLanguage.Key, steamLanguage.Value, gameInfo));
                    }

                    var gameInfos = (await Task.WhenAll(categoriesList)).SelectMany(c => c);
                }
            }
        }

        public async Task<GameInfo[]> FetchCategoryInfo(HelixCategoriesSearchEntry category, string twitchLanguage, string steamLanguage, GameInfo gameInfo)
        {
            var resultList = new List<GameInfo>();

            if (! gameInfo.IGDBId.HasValue)
            {
                _logger.LogInformation("Category: {categoryName}, id: {categoryId}", category.Name, category.Id);
                var igdbExternalEntry = await _igdbClient.SearchExternalGame("uid", $"\"{category.Id}\"", IGDBExternalGameCategory.Twitch);
                if (igdbExternalEntry.Length == 0)
                {
                    _logger.LogInformation("No IGDB entry");
                    return new GameInfo[0];
                }
                gameInfo.IGDBId = igdbExternalEntry.First().Game.Id;

                _logger.LogInformation("Found IGDB entry: {igdbName}, {igdbGameId}", igdbExternalEntry.First().Name, igdbExternalEntry.First().Game.Id);

                var igdbEntry = await _igdbClient.GetGameByIdAsync(gameInfo.IGDBId.Value);
                if (igdbEntry == null)
                {
                    _logger.LogInformation("Failed to resolve IGDB entry");
                    return new GameInfo[0];
                }
                gameInfo.IGDBId = igdbEntry.ParentGame?.Id ?? igdbEntry.Id;
                gameInfo.Summary = igdbEntry.Summary;

                _logger.LogInformation("Resolved IGDB entry: {igdbName}, {igdbGameId}", igdbEntry.Name, igdbExternalEntry.First().Game.Id);
            }
            else
            {
                _logger.LogInformation("Using existing IGDB entry: {igdbGameId}", gameInfo.IGDBId);
            }

            var igdbSteamEntry = await _igdbClient.SearchExternalGame("game", gameInfo.IGDBId.ToString(), IGDBExternalGameCategory.Steam);
            if (igdbSteamEntry.Length > 0)
            {
                _logger.LogInformation("Found a Steam entry !");
                var storeEntry = await _steamStoreClient.GetStoreDetails(igdbSteamEntry.First().Uid, steamLanguage);
                if (storeEntry != null)
                {
                    var localizedGameInfo = new GameInfo
                    {
                        TwitchCategoryId = gameInfo.TwitchCategoryId,
                        Name = gameInfo.Name,
                        IGDBId = gameInfo.IGDBId,
                        Summary = storeEntry.ShortDescription,
                        Language = twitchLanguage,
                        Source = "steam",
                    };

                    resultList.Add(localizedGameInfo);
                }
            }
            else
            {
                resultList.Add(gameInfo);
            }

            return resultList.ToArray();
        }
    }
}
