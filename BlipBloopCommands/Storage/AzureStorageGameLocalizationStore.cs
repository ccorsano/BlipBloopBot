﻿using BlipBloopBot.Storage;
using Conceptoire.Twitch.Model;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopCommands.Storage
{
    public class AzureStorageGameLocalizationStore : IGameLocalizationStore
    {
        private readonly AzureGameLocalizationStoreOptions _options;
        private readonly ILogger _logger;

        private CloudStorageAccount _account;
        private CloudTableClient _tableClient;
        private CloudTable _table;

        public AzureStorageGameLocalizationStore(IOptions<AzureGameLocalizationStoreOptions> options, ILogger<AzureStorageGameLocalizationStore> logger)
        {
            _options = options.Value;
            _logger = logger;
            _account = CloudStorageAccount.Parse(_options.StorageConnectionString);
            _tableClient = _account.CreateCloudTableClient();
            _table = _tableClient.GetTableReference(_options.TableName);
        }

        public async Task<GameInfo> ResolveLocalizedGameInfo(string language, string twitchCategoryId)
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<GameLocalizationInfoEntity>(twitchCategoryId, language);
                TableResult result = await _table.ExecuteAsync(retrieveOperation);
                GameLocalizationInfoEntity gameInfoEntity = result.Result as GameLocalizationInfoEntity;
                if (gameInfoEntity == null)
                {
                    return null;
                }

                return gameInfoEntity.ToGameInfo();
            }
            catch (StorageException e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        public async Task SaveGameInfo(GameInfo gameInfo)
        {
            try
            {
                var gameInfoEntity = new GameLocalizationInfoEntity(gameInfo);
                var operation = TableOperation.InsertOrMerge(gameInfoEntity);
                var result = await _table.ExecuteAsync(operation);
                if (result.HttpStatusCode / 100 != 2)
                {
                    _logger.LogError("Non success code on insert {httpStatus}", result.HttpStatusCode);
                    throw new Exception("Error http code on insert");
                }
            }
            catch (StorageException e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }
    }

    public class GameLocalizationInfoEntity : TableEntity
    {
        public GameLocalizationInfoEntity()
        {
        }

        public GameLocalizationInfoEntity(string gameId, string locale) : base(gameId, locale)
        {
        }

        public GameLocalizationInfoEntity(GameInfo gameInfo) : base(gameInfo.TwitchCategoryId, gameInfo.Language)
        {
            Name = gameInfo.Name;
            Synopsis = gameInfo.Synopsis;
            Summary = gameInfo.Summary;
            Source = gameInfo.Source;

            if (gameInfo.IGDBId.HasValue && gameInfo.IGDBId > long.MaxValue)
            {
                throw new ArgumentOutOfRangeException($"IGDB Id {gameInfo.IGDBId} exceeds the supported storage type for Azure Table");
            }
            IGDBId = (long?) gameInfo.IGDBId;

            if (gameInfo.SteamId.HasValue && gameInfo.SteamId > long.MaxValue)
            {
                throw new ArgumentOutOfRangeException($"Steam Id {gameInfo.SteamId} exceeds the supported storage type for Azure Table");
            }
            SteamId = (long?) gameInfo.SteamId;
        }

        public string TwitchCategoryId => PartitionKey;
        public string Language => RowKey;
        public string Name { get; set; }
        public string Synopsis { get; set; }
        public string Summary { get; set; }
        public string Source { get; set; }
        public long? IGDBId { get; set; }
        public long? SteamId { get; set; }

        public GameInfo ToGameInfo()
        {
            if (IGDBId.HasValue && IGDBId < 0)
            {
                throw new ArgumentOutOfRangeException($"Negative IGDB Id {IGDBId} found on entity {TwitchCategoryId}:{Language}");
            }

            if (SteamId.HasValue && SteamId < 0)
            {
                throw new ArgumentOutOfRangeException($"Negative Steam Id {SteamId} found on entity {TwitchCategoryId}:{Language}");
            }
            return new GameInfo
            {
                TwitchCategoryId = TwitchCategoryId,
                Language = Language,
                Name = Name,
                Summary = Summary,
                Synopsis = Synopsis,
                Source = Source,
                IGDBId = (ulong?) IGDBId,
                SteamId = (ulong?) SteamId,
            };
        }
    }
}
