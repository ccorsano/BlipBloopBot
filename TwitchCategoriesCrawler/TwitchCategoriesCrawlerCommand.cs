﻿using Conceptoire.Twitch.Constants;
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
using BlipBloopCommands.Storage;
using BlipBloopBot.Storage;
using BlipBloopCommands;

namespace TwitchCategoriesCrawler
{
    [Command("crawl", Description = "Crawl Twitch categories and resolve localization from Steam")]
    [HelpOption]
    public class TwitchCategoriesCrawlerCommand
    {
        private readonly TwitchAPIClient _twitchAPIClient;
        private readonly IGDBClient _igdbClient;
        private readonly SteamStoreClient _steamStoreClient;
        private readonly TwitchApplicationOptions _options;
        private readonly IGameLocalizationStore _gameLocalization;
        private readonly ILogger _logger;

        [Option("-l", CommandOptionType.SingleValue, Description = "Language to import from Steam")]
        public string TargetLanguage { get; set; }

        [Option("-f", CommandOptionType.SingleOrNoValue, Description = "Force update of existing entries")]
        public bool Force { get; set; }

        public TwitchCategoriesCrawlerCommand(
            TwitchAPIClient twitchApiClient,
            IGDBClient igdbClient,
            SteamStoreClient steamStoreClient,
            IOptions<TwitchApplicationOptions> options,
            IGameLocalizationStore gameLocalizationStore,
            ILogger<TwitchCategoriesCrawlerCommand> logger)
        {
            _twitchAPIClient = twitchApiClient;
            _igdbClient = igdbClient;
            _steamStoreClient = steamStoreClient;
            _steamStoreClient.WebAPIKey = options.Value.SteamApiKey;
            _gameLocalization = gameLocalizationStore;
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
                    csvReader.Context.RegisterClassMap<GameInfoCsvClassMap>();
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

            uint streamCount = 0;
            using (var textWriter = new StreamWriter("gamedb.csv", true))
            using (var csvWriter = new CsvWriter(textWriter, configuration))
            {
                csvWriter.Context.RegisterClassMap<GameInfoCsvClassMap>();
                var steamLanguage = SteamConstants.TwitchLanguageMapping.First(tlm => tlm.Key == TargetLanguage);
                //foreach (var steamLanguage in SteamConstants.TwitchLanguageMapping)
                {
                    // Scan all categories
                    await foreach (HelixGetStreamsEntry stream in _twitchAPIClient.EnumerateStreamsAsync())
                    {
                        streamCount++;
                        _logger.LogWarning("Stream {index}, {viewerCount}: {streamer} {categoryName}", streamCount, stream.ViewerCount, stream.UserName, stream.GameName);
                        GameInfo gameInfo = null;

                        if (_gameLocalization != null)
                        {
                            var existingBaseEntry = await _gameLocalization.ResolveLocalizedGameInfoAsync("", stream.GameId);
                            if (!Force && existingBaseEntry != null && !existingBaseEntry.IGDBId.HasValue)
                            {
                                _logger.LogWarning("Found existing persisted base entry, no IGDB record, skipping");
                                continue;
                            }
                            gameInfo = existingBaseEntry;

                            var existingLog = await _gameLocalization.ResolveLocalizedGameInfoAsync(TargetLanguage, stream.GameId);
                            if (!Force && gameInfo != null && existingLog != null)
                            {
                                _logger.LogWarning("Found existing persisted base entry, no IGDB record, skipping");
                                continue;
                            }
                        }
                        else
                        {
                            if (gameDb.TryGetValue((stream.GameId, steamLanguage.Key), out gameInfo))
                            {
                                _logger.LogWarning("Found existing entry {categoryId}, {categoryName}, {language}", stream.GameId, stream.GameName, steamLanguage.Key);
                                continue;
                            }

                            if (gameDb.TryGetValue((stream.GameId, TwitchConstants.LanguageCodes.ENGLISH), out gameInfo))
                            {
                                _logger.LogWarning("Found existing base EN entry for {categoryId}, {categoryName}", stream.GameId, stream.GameName);
                            }
                            else
                            {
                                if (steamLanguage.Key != TwitchConstants.LanguageCodes.ENGLISH && !gameDb.TryAdd((stream.GameId, TwitchConstants.LanguageCodes.ENGLISH), gameInfo))
                                {
                                    _logger.LogWarning("Received same category twice {categoryId} ({categoryName})", stream.GameId, stream.GameName);
                                }
                            }
                        }

                        if (gameInfo == null)
                        {
                            gameInfo = new GameInfo
                            {
                                TwitchCategoryId = stream.GameId,
                                Language = "",
                                Name = stream.GameName,
                            };
                        }

                        // If we already resolved that category for this language, skip it
                        if (gameDb.ContainsKey((stream.GameId, steamLanguage.Key)))
                        {
                            _logger.LogWarning("Found existing localized {language} entry for {categoryId}, {categoryName}", steamLanguage.Key, stream.GameId, stream.GameName);
                            continue;
                        }

                        var updatedGameInfo = await FetchCategoryInfo(stream, steamLanguage.Key, steamLanguage.Value, gameInfo);
                        csvWriter.WriteRecords(updatedGameInfo);

                        if (_gameLocalization != null)
                        {
                            await Task.WhenAll(updatedGameInfo.Select(gameInfo => _gameLocalization.SaveGameInfoAsync(gameInfo)));
                        }

                        if (stream.ViewerCount < 50)
                        {
                            _logger.LogWarning("Stopped iterating after {streamCount}", streamCount);
                            break;
                        }
                    }

                    var gameInfos = (await Task.WhenAll(categoriesList)).SelectMany(c => c);
                }
            }
        }

        public async Task<GameInfo[]> FetchCategoryInfo(HelixGetStreamsEntry stream, string twitchLanguage, string steamLanguage, GameInfo gameInfo)
        {
            var resultList = new List<GameInfo>();

            var hasSteamEntry = gameInfo.IGDBId.HasValue && gameInfo.SteamId.HasValue;

            if (Force || !gameInfo.IGDBId.HasValue)
            {
                _logger.LogInformation("Category: {categoryName}, id: {categoryId}", stream.GameName, stream.GameId);
                var igdbExternalEntry = await _igdbClient.SearchExternalGame("uid", $"\"{stream.GameId}\"", IGDBExternalGameCategory.Twitch);
                if (igdbExternalEntry.Length == 0)
                {
                    _logger.LogInformation("No IGDB entry");
                    return new GameInfo[] { gameInfo };
                }
                gameInfo.IGDBId = igdbExternalEntry.First().Game.Id;

                _logger.LogInformation("Found IGDB entry: {igdbName}, {igdbGameId}", igdbExternalEntry.First().Name, igdbExternalEntry.First().Game.Id);

                var igdbEntry = await _igdbClient.GetGameByIdAsync(gameInfo.IGDBId.Value);
                if (igdbEntry == null)
                {
                    _logger.LogInformation("Failed to resolve IGDB entry");
                    return new GameInfo[] { gameInfo };
                }
                gameInfo.IGDBId = igdbEntry.ParentGame?.Id ?? igdbEntry.Id;
                gameInfo.Summary = igdbEntry.Summary;

                _logger.LogInformation("Resolved IGDB entry: {igdbName}, {igdbGameId}", igdbEntry.Name, igdbExternalEntry.First().Game.Id);
                var igdbSteamEntry = await _igdbClient.SearchExternalGame("game", gameInfo.IGDBId.ToString(), IGDBExternalGameCategory.Steam);
                if (igdbSteamEntry.Length > 0)
                {
                    gameInfo.SteamId = ulong.Parse(igdbSteamEntry.First().Uid);
                    hasSteamEntry = true;
                    _logger.LogInformation("Found a Steam entry !");
                }
                resultList.Add(gameInfo);
            }
            else
            {
                _logger.LogInformation("Using existing IGDB entry: {igdbGameId}", gameInfo.IGDBId);
            }

            if (hasSteamEntry)
            {
                _logger.LogInformation("Found a Steam entry !");
                var storeEntry = await _steamStoreClient.GetStoreDetails(gameInfo.SteamId.ToString(), steamLanguage);
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
                        SteamId = storeEntry.SteamAppid
                    };

                    resultList.Add(localizedGameInfo);
                }
            }

            return resultList.ToArray();
        }
    }
}
