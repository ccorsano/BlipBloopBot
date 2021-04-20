using BlipBloopBot.Storage;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Constants;
using Conceptoire.Twitch.Steam;
using CsvHelper;
using CsvHelper.Configuration;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchCategoriesCrawler
{
    [Command("export", Description = "Export the online database to a file")]
    [HelpOption]
    public class ExportCommand
    {
        private readonly IGameLocalizationStore _gameLocalization;
        private readonly ILogger _logger;

        [Option("-l", CommandOptionType.MultipleValue, Description = "Language to import from Steam")]
        public string[] TargetLanguages { get; set; }

        [Option("-o", CommandOptionType.SingleValue, Description = "Language to import from Steam")]
        [FileNotExists, LegalFilePath]
        public string OutFile { get; set; }

        public ExportCommand(
            IGameLocalizationStore gameLocalizationStore,
            ILogger<ExportCommand> logger)
        {
            _gameLocalization = gameLocalizationStore;
            _logger = logger;
        }

        private async Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
            };

            if (! (TargetLanguages?.Any() ?? false))
            {
                TargetLanguages = SteamConstants.SupportedLanguages.ToArray();
            }

            using (var textWriter = new StreamWriter(OutFile, false))
            using (var csvWriter = new CsvWriter(textWriter, configuration))
            {
                foreach(var targetLanguage in TargetLanguages)
                {
                    _logger.LogInformation("Enumerating all generic category entries");
                    var entryCount = 0;
                    await foreach (var gameInfo in _gameLocalization.EnumerateGameInfoAsync(targetLanguage, cancellationToken))
                    {
                        entryCount++;
                        csvWriter.WriteRecord(gameInfo);
                        csvWriter.NextRecord();
                    }
                    _logger.LogInformation("Exported {totalCount} localized category entries for language {language}", entryCount, targetLanguage);
                }
            }
        }
    }
}
