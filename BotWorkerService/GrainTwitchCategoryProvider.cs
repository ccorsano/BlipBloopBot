using BlipBloopBot.Storage;
using BlipBloopCommands.Commands.GameSynopsis;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Constants;
using Conceptoire.Twitch.Model;
using Conceptoire.Twitch.Steam;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotServiceGrain
{
    public class GrainTwitchCategoryProvider : ITwitchCategoryProvider
    {
        private readonly IGrainFactory _grainFactory;
        private readonly TwitchAPIClient _twitchAPIClient;
        private readonly IGDBClient _igdbClient;
        private readonly SteamStoreClient _steamStoreClient;
        private readonly IGameLocalizationStore _gameLocalizationStore;
        private readonly ILogger _logger;

        public event EventHandler<GameInfo> OnUpdate;

        public GrainTwitchCategoryProvider(
            IGrainFactory grainFactory,
            TwitchAPIClient twitchClient,
            IGDBClient igdbClient,
            SteamStoreClient steamStoreClient,
            IGameLocalizationStore localizationStore,
            ILogger<GrainTwitchCategoryProvider> logger)
        {
            _grainFactory = grainFactory;
            _twitchAPIClient = twitchClient;
            _igdbClient = igdbClient;
            _steamStoreClient = steamStoreClient;
            _gameLocalizationStore = localizationStore;
            _logger = logger;
        }

        async Task<GameInfo> ITwitchCategoryProvider.FetchChannelInfo(string categoryId, string language, System.Threading.CancellationToken cancellationToken)
        {
            var localized = await _gameLocalizationStore.ResolveLocalizedGameInfoAsync(language, categoryId, cancellationToken);
            if (localized == null)
            {
                localized = await ResolveCategory(categoryId, language, cancellationToken);
            }
            if (OnUpdate != null)
            {
                OnUpdate(this, localized);
            }
            return localized;
        }


        private async Task<GameInfo> ResolveCategory(string categoryId, string language, CancellationToken cancellationToken)
        {
            var gameInfo = new GameInfo
            {
                TwitchCategoryId = categoryId,
                Language = language,
            };

            _logger.LogWarning("No result for localized info for {gameId}, looking up base record", categoryId);
            var baseInfo = await _gameLocalizationStore.ResolveLocalizedGameInfoAsync("", categoryId);

            if (baseInfo != null && (!baseInfo.IGDBId.HasValue || !baseInfo.SteamId.HasValue))
            {
                _logger.LogWarning("Base record for {gameId} exists, but is not linked to a Steam appid.", categoryId);
                return gameInfo;
            }

            if (baseInfo == null)
            {
                baseInfo = await ResolveBaseRecord(categoryId, language, cancellationToken);
                await _gameLocalizationStore.SaveGameInfoAsync(baseInfo, cancellationToken);
            }

            if (baseInfo != null)
            {
                gameInfo.Name = baseInfo.Name;
                gameInfo.IGDBId = baseInfo.IGDBId;
                gameInfo.SteamId = baseInfo.SteamId;
                gameInfo.Summary = baseInfo.Summary;
                gameInfo.Synopsis = baseInfo.Synopsis;
                gameInfo.Source = baseInfo.Source;
            }

            if (baseInfo != null && baseInfo.SteamId.HasValue)
            {
                _logger.LogWarning("Found an existing base record for {gameId} with associated Steam appid {}.", categoryId, baseInfo.SteamId);
                var steamLang = SteamConstants.TwitchLanguageMapping[language];
                var storeDetails = await _steamStoreClient.GetStoreDetails(baseInfo.SteamId.ToString(), steamLang);
                if (storeDetails != null)
                {
                    gameInfo = new GameInfo
                    {
                        TwitchCategoryId = baseInfo.TwitchCategoryId,
                        Name = baseInfo.Name,
                        IGDBId = baseInfo.IGDBId,
                        Summary = storeDetails.ShortDescription,
                        Language = language,
                        Source = "steam",
                        SteamId = baseInfo.SteamId,
                    };
                }
            }
            return gameInfo;
        }

        private async Task<GameInfo> ResolveBaseRecord(string categoryId, string language, CancellationToken cancellationToken)
        {
            var baseInfo = new GameInfo
            {
                TwitchCategoryId = categoryId,
                Language = "",
            };

            _logger.LogInformation("Category id: {categoryId}", categoryId);
            var igdbExternalEntry = await _igdbClient.SearchExternalGame("uid", $"\"{categoryId}\"", IGDBExternalGameCategory.Twitch);
            if (igdbExternalEntry.Length == 0)
            {
                _logger.LogWarning("No IGDB entry");
                return baseInfo;
            }
            baseInfo.IGDBId = igdbExternalEntry.First().Game.Id;
            baseInfo.Name = baseInfo.Name ?? igdbExternalEntry.First().Name;

            _logger.LogInformation("Found IGDB entry: {igdbName}, {igdbGameId}", igdbExternalEntry.First().Name, igdbExternalEntry.First().Game.Id);

            var igdbEntry = await _igdbClient.GetGameByIdAsync(baseInfo.IGDBId.Value);
            if (igdbEntry == null)
            {
                _logger.LogInformation("Failed to resolve IGDB entry");
                return baseInfo;
            }
            baseInfo.IGDBId = igdbEntry.ParentGame?.Id ?? igdbEntry.Id;
            baseInfo.Summary = igdbEntry.Summary;

            _logger.LogInformation("Resolved IGDB entry: {igdbName}, {igdbGameId}", igdbEntry.Name, igdbExternalEntry.First().Game.Id);
            var igdbSteamEntry = await _igdbClient.SearchExternalGame("game", baseInfo.IGDBId.ToString(), IGDBExternalGameCategory.Steam);
            if (igdbSteamEntry.Length > 0)
            {
                baseInfo.SteamId = ulong.Parse(igdbSteamEntry.First().Uid);
                _logger.LogInformation("Found a Steam entry !");
            }

            return baseInfo;
        }
    }
}
