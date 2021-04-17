using BlipBloopBot.Storage;
using BlipBloopCommands.Commands.GameSynopsis;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Model;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrain
{
    public class GrainTwitchCategoryProvider : ITwitchCategoryProvider
    {
        private readonly IGrainFactory _grainFactory;
        private readonly TwitchAPIClient _twitchAPIClient;
        private readonly IGameLocalizationStore _gameLocalizationStore;
        private readonly ILogger _logger;

        public event EventHandler<GameInfo> OnUpdate;

        public GrainTwitchCategoryProvider(
            IGrainFactory grainFactory,
            TwitchAPIClient twitchClient,
            IGameLocalizationStore localizationStore,
            ILogger<GrainTwitchCategoryProvider> logger)
        {
            _grainFactory = grainFactory;
            _twitchAPIClient = twitchClient;
            _gameLocalizationStore = localizationStore;
            _logger = logger;
        }

        async Task<GameInfo> ITwitchCategoryProvider.FetchChannelInfo(string categoryId, string language)
        {
            var localized = await _gameLocalizationStore.ResolveLocalizedGameInfoAsync(language, categoryId);
            if (OnUpdate != null)
            {
                OnUpdate(this, localized);
            }
            return localized;
        }
    }
}
