using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using BlipBloopBot.Storage;
using Conceptoire.Twitch.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlipBloopCommands.Storage
{
    public class AzureStorageGameLocalizationStore : IGameLocalizationStore
    {
        private readonly AzureGameLocalizationStoreOptions _options;
        private readonly ILogger _logger;

        private TableServiceClient _tableServiceClient;
        private TableClient _tableClient;

        public AzureStorageGameLocalizationStore(IOptions<AzureGameLocalizationStoreOptions> options, ILogger<AzureStorageGameLocalizationStore> logger)
        {
            _options = options.Value;
            _logger = logger;
            _tableServiceClient = new TableServiceClient(_options.StorageConnectionString);
            _tableClient = _tableServiceClient.GetTableClient(_options.TableName);
        }

        async Task<GameInfo> IGameLocalizationStore.ResolveLocalizedGameInfoAsync(string language, string twitchCategoryId, CancellationToken cancellationToken)
        {
            try
            {
                NullableResponse<GameLocalizationInfoEntity> response = await _tableClient.GetEntityIfExistsAsync<GameLocalizationInfoEntity>(twitchCategoryId, language, null, cancellationToken);
                if (! response.HasValue)
                {
                    return null;
                }

                return response.Value.ToGameInfo();
            }
            catch (RequestFailedException e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        async Task IGameLocalizationStore.SaveGameInfoAsync(GameInfo gameInfo, CancellationToken cancellationToken)
        {
            try
            {
                var gameInfoEntity = new GameLocalizationInfoEntity(gameInfo);
                var result = await _tableClient.UpsertEntityAsync(gameInfoEntity, TableUpdateMode.Merge, cancellationToken);
                if (result.IsError)
                {
                    _logger.LogError("Non success code on insert {httpStatus}", result.Status);
                    throw new Exception("Error http code on insert");
                }
            }
            catch (RequestFailedException e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        async IAsyncEnumerable<GameInfo> IGameLocalizationStore.EnumerateGameInfoAsync(string language, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            AsyncPageable<GameLocalizationInfoEntity> query = _tableClient
                .QueryAsync<GameLocalizationInfoEntity>(e => e.RowKey == language, 1000);
            
            await foreach (Page<GameLocalizationInfoEntity> page in query.AsPages())
            {
                foreach (GameLocalizationInfoEntity value in page.Values)
                {
                    yield return value.ToGameInfo();
                }
            }
        }
    }

    public class GameLocalizationInfoEntity : ITableEntity
    {
        public GameLocalizationInfoEntity()
        {
        }

        public GameLocalizationInfoEntity(string gameId, string locale)
        {
            PartitionKey = gameId;
            RowKey = locale;
        }

        public GameLocalizationInfoEntity(GameInfo gameInfo)
        {
            PartitionKey = gameInfo.TwitchCategoryId;
            RowKey = gameInfo.Language;

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

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

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
