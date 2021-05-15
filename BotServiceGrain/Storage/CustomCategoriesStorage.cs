using BotServiceGrainInterface;
using BotServiceGrainInterface.Model;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotServiceGrain.Storage
{
    public class CustomCategoryEntity : TableEntity
    {
        public CustomCategoryEntity() { }

        public CustomCategoryEntity(string channelId, string categoryId, string locale)
            :base(channelId, $"{categoryId}:{locale}")
        {
            CategoryId = categoryId;
            Locale = locale;
        }

        public string Description { get; set; }

        [IgnoreProperty]
        public string ChannelId => PartitionKey;

        [IgnoreProperty]
        public string CategoryId { get; private set; }
        
        [IgnoreProperty]
        public string Locale { get; private set; }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            var split = RowKey.Split(':');
            CategoryId = split[0];
            Locale = split[1];
        }
    }

    public class CustomCategoriesStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly string _storageName;
        private readonly CustomCategoriesStorageOptions _storageOptions;
        private ILogger _logger;
        private CloudStorageAccount _account;
        private CloudTableClient _tableClient;
        private CloudTable _table;

        public CustomCategoriesStorage(string storageName,
            CustomCategoriesStorageOptions storageOptions,
            ILogger<CustomCategoriesStorage> logger)
        {
            _storageName = storageName;
            _storageOptions = storageOptions;
            _logger = logger;
        }

        private async Task Init(CancellationToken ct)
        {
            _account = CloudStorageAccount.Parse(_storageOptions.ConnectionString);
            _tableClient = _account.CreateCloudTableClient();
            _table = _tableClient.GetTableReference(_storageOptions.TableName);
            await _table.CreateIfNotExistsAsync();
        }

        public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var entityList = await FetchAll(grainReference);
            try
            {
                var batch = new TableBatchOperation();
                foreach(var entity in entityList)
                {
                    batch.Delete(entity);
                }
                await _table.ExecuteBatchAsync(batch);
            }
            catch (StorageException e)
            {
                _logger.LogError(e, "Could not clear state");
                throw;
            }
        }

        private async Task<List<CustomCategoryEntity>> FetchAll(GrainReference grainReference)
        {
            try
            {
                var query = new TableQuery<CustomCategoryEntity>();
                query.FilterString = TableQuery.GenerateFilterCondition("PartitionKey", "eq", grainReference.GetPrimaryKeyString());

                TableQuerySegment<CustomCategoryEntity> querySegment = null;
                var entityList = new List<CustomCategoryEntity>();
                while (querySegment == null || querySegment.ContinuationToken != null)
                {
                    querySegment = await _table.ExecuteQuerySegmentedAsync(query, querySegment != null ?
                                                     querySegment.ContinuationToken : null);
                    entityList.AddRange(querySegment);
                }

                return entityList;
            }
            catch (StorageException e)
            {
                _logger.LogError(e, "Failed to read state from Table Storage");
                throw;
            }
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(OptionFormattingUtilities.Name<CustomCategoriesStorage>(_storageName), ServiceLifecycleStage.ApplicationServices, Init);
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var entities = await FetchAll(grainReference);
            
            var state = (CategoryDescriptionState) grainState.State;
            var descriptions = entities.Select(e => new CustomCategoryDescription
            {
                TwitchCategoryId = e.CategoryId,
                Locale = e.Locale,
                Description = e.Description,
            });
            state.Descriptions = descriptions.ToDictionary(d => new CategoryKey { TwitchCategoryId = d.TwitchCategoryId, Locale = d.Locale }, d => d);
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var entities = await FetchAll(grainReference);

            var state = (CategoryDescriptionState)grainState.State;

            var removedEntries = entities.Where(e => !state.Descriptions.ContainsKey(new CategoryKey
            {
                TwitchCategoryId = e.CategoryId,
                Locale = e.Locale,
            }));
            var createdEntries = state.Descriptions.Values.Where(e => !entities.Any(s => s.CategoryId == e.TwitchCategoryId && s.Locale == e.Locale));

            var batchOp = new TableBatchOperation();
            foreach(var deleted in removedEntries)
            {
                batchOp.Delete(deleted);
            }
            foreach(var description in state.Descriptions.Values)
            {
                var entity = entities.FirstOrDefault(e => e.CategoryId == description.TwitchCategoryId && e.Locale == description.Locale);
                if (entity == null)
                {
                    entity = new CustomCategoryEntity(grainReference.GetPrimaryKeyString(), description.TwitchCategoryId, description.Locale);
                }
                if (entity.Description == description.Description)
                {
                    continue;
                }
                entity.Description = description.Description;
                batchOp.InsertOrReplace(entity);
            }
            await _table.ExecuteBatchAsync(batchOp);
        }
    }
}
