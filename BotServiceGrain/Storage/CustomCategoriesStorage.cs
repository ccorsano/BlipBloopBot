using Azure;
using Azure.Data.Tables;
using BotServiceGrainInterface;
using BotServiceGrainInterface.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotServiceGrain.Storage
{
    public class CustomCategoryEntity : ITableEntity
    {
        private string _rowKey;

        public CustomCategoryEntity() { }

        public CustomCategoryEntity(string channelId, string categoryId, string locale)
        {
            PartitionKey = channelId;
            _rowKey = $"{categoryId}:{locale}";
            CategoryId = categoryId;
            Locale = locale;
        }
        public string PartitionKey { get; set; }
        public string RowKey {
            get => _rowKey;
            set
            {
                var split = RowKey.Split(':');
                CategoryId = split[0];
                Locale = split[1];
            }
        }

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string Description { get; set; }

        [IgnoreDataMember]
        public string ChannelId => PartitionKey;

        [IgnoreDataMember]
        public string CategoryId { get; private set; }
        
        [IgnoreDataMember]
        public string Locale { get; private set; }
    }

    public class CustomCategoriesStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly string _storageName;
        private readonly CustomCategoriesStorageOptions _storageOptions;
        private ILogger _logger;
        private TableServiceClient _tableClient;
        private TableClient _table;

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
            _tableClient = new TableServiceClient(_storageOptions.ConnectionString);
            _table = _tableClient.GetTableClient(_storageOptions.TableName);
            await _table.CreateIfNotExistsAsync();
        }

        public async Task ClearStateAsync<T>(string grainType, GrainId grainId, IGrainState<T> grainState)
        {
            var entityList = await FetchAll(grainId);
            try
            {
                List<TableTransactionAction> deleteBatch = entityList.Select(e => new TableTransactionAction(TableTransactionActionType.Delete, e)).ToList();
                await _table.SubmitTransactionAsync(deleteBatch);
            }
            catch (Exception e) when (e is RequestFailedException || e is InvalidOperationException)
            {
                _logger.LogError(e, "Could not clear state");
                throw;
            }
        }

        private async Task<List<CustomCategoryEntity>> FetchAll(GrainId grainId)
        {
            try
            {
                AsyncPageable<CustomCategoryEntity> query = _table.QueryAsync<CustomCategoryEntity>(e => e.PartitionKey == grainId.Key.ToString());

                var entityList = new List<CustomCategoryEntity>();
                await foreach (var entity in query)
                {
                    entityList.Add(entity);
                }

                return entityList;
            }
            catch (RequestFailedException e)
            {
                _logger.LogError(e, "Failed to read state from Table Storage");
                throw;
            }
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(OptionFormattingUtilities.Name<CustomCategoriesStorage>(_storageName), ServiceLifecycleStage.ApplicationServices, Init);
        }

        public async Task ReadStateAsync<T>(string grainType, GrainId grainReference, IGrainState<T> grainState)
        {
            if (typeof(T) != typeof(CategoryDescriptionState))
            {
                throw new InvalidOperationException("ReadState called with unexpected type");
            }
            var entities = await FetchAll(grainReference);
            
            var state = grainState.State as CategoryDescriptionState;
            var descriptions = entities.Select(e => new CustomCategoryDescription
            {
                TwitchCategoryId = e.CategoryId,
                Locale = e.Locale,
                Description = e.Description,
            });
            state.Descriptions = descriptions.ToDictionary(d => new CategoryKey { TwitchCategoryId = d.TwitchCategoryId, Locale = d.Locale }, d => d);
        }

        public async Task WriteStateAsync<T>(string grainType, GrainId grainReference, IGrainState<T> grainState)
        {
            if (typeof(T) != typeof(CategoryDescriptionState))
            {
                throw new InvalidOperationException("ReadState called with unexpected type");
            }
            var entities = await FetchAll(grainReference);

            var state = grainState.State as CategoryDescriptionState;

            var removedEntries = entities.Where(e => !state.Descriptions.ContainsKey(new CategoryKey
            {
                TwitchCategoryId = e.CategoryId,
                Locale = e.Locale,
            }));
            var createdEntries = state.Descriptions.Values.Where(e => !entities.Any(s => s.CategoryId == e.TwitchCategoryId && s.Locale == e.Locale));

            List<TableTransactionAction> batchOperations = new();
            batchOperations.AddRange(removedEntries.Select(e => new TableTransactionAction(TableTransactionActionType.Delete, e)));

            foreach(var description in state.Descriptions.Values)
            {
                var entity = entities.FirstOrDefault(e => e.CategoryId == description.TwitchCategoryId && e.Locale == description.Locale);
                if (entity == null)
                {
                    entity = new CustomCategoryEntity(grainReference.Key.ToString(), description.TwitchCategoryId, description.Locale);
                }
                if (entity.Description == description.Description)
                {
                    continue;
                }
                entity.Description = description.Description;
                batchOperations.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, entity));
            }
            await _table.SubmitTransactionAsync(batchOperations);
        }
    }
}
