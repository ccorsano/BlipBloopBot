using Conceptoire.Twitch.Model;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace BlipBloopBot.Storage
{
    public class EmbeddedGameLocalizationDb : IGameLocalizationStore
    {
        private IDictionary<(string, string), GameInfo> _localizedInfo = new Dictionary<(string, string), GameInfo>();
        private readonly Task _loaderTask;
        private readonly ILogger _logger;

        public EmbeddedGameLocalizationDb(ILogger<EmbeddedGameLocalizationDb> logger)
        {
            _loaderTask = Task.Run(LoadEmbeddedCSVFile);
            _logger = logger;
        }

        private async Task LoadEmbeddedCSVFile()
        {
            var file = Assembly.GetAssembly(this.GetType()).GetManifestResourceStream("BlipBloopCommands.gamedb_fr.csv");
            if (file == null)
            {
                throw new Exception("Could not find embedded game localization data");
            }


            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
            };
            using (var streamReader = new StreamReader(file))
            using (var csvReader = new CsvReader(streamReader, configuration))
            {
                await foreach (var gameRecord in csvReader.GetRecordsAsync<GameInfo>())
                {
                    if (!_localizedInfo.TryAdd((gameRecord.Language, gameRecord.TwitchCategoryId), gameRecord))
                    {
                        _logger.LogInformation("Ignoring duplicate localized game record {language}, {twitchId} ({gameName})", gameRecord.Language, gameRecord.TwitchCategoryId, gameRecord.Name);
                    }
                }
            }
        }

        public async Task<GameInfo> ResolveLocalizedGameInfoAsync(string language, string twitchCategoryId)
        {
            await _loaderTask;

            if (_localizedInfo.TryGetValue((language, twitchCategoryId), out var gameInfo))
            {
                return gameInfo;
            }

            return null;
        }

        public Task SaveGameInfoAsync(GameInfo gameInfo)
        {
            throw new NotImplementedException();
        }
    }
}
