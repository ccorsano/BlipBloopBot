using BlipBloopBot.Storage;
using Conceptoire.Twitch.Model;
using Conceptoire.Twitch;
using Conceptoire.Twitch.API;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlipBloopCommands.Commands.GameSynopsis
{
    public class PollingTwitchCategoryProvider : ITwitchCategoryProvider, IDisposable
    {
        private readonly TwitchAPIClient _twitchAPIClient;
        private readonly IGameLocalizationStore _gameLocalization;
        private readonly TwitchApplicationOptions _twitchOptions;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private string _channelId;
        private HelixChannelSearchResult _lastResult;
        private HelixChannelInfo _lastInfo;
        private GameInfo _gameInfo;

        private DateTime _lastCheck;

        public event EventHandler<GameInfo> OnUpdate;

        public PollingTwitchCategoryProvider(
            TwitchAPIClient twitchAPIClient,
            IGameLocalizationStore gameLocalizationStore,
            IOptions<TwitchApplicationOptions> twitchOptions,
            ILogger<PollingTwitchCategoryProvider> logger)
        {
            _twitchAPIClient = twitchAPIClient;
            _gameLocalization = gameLocalizationStore;
            _twitchOptions = twitchOptions.Value;
            _logger = logger;

            var unawaited = Task.Run(async () =>
            {
                while(!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await RefreshCategory(_channelId);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            });
        }

        public bool CheckAndSchedule(string channelId)
        {
            if (channelId != _channelId)
            {
                _lastCheck = DateTime.MinValue;
                _channelId = channelId;
                return false;
            }

            return true;
        }

        public async Task<GameInfo> RefreshCategory(string broadcasterId)
        {
            _logger.LogInformation("Starting category refresh for channelId {broadcasterId}", broadcasterId);

            try
            {
                var results = await _twitchAPIClient.SearchChannelsAsync(broadcasterId);
                var newResults = results.First(c => c.BroadcasterLogin == broadcasterId);

                if (newResults.GameId != _lastResult?.GameId)
                {
                    _lastInfo = await _twitchAPIClient.GetChannelInfoAsync(newResults.Id);
                    _gameInfo = await _gameLocalization.ResolveLocalizedGameInfo(newResults.BroadcasterLanguage, newResults.GameId);

                    if (OnUpdate != null)
                    {
                        OnUpdate(this, _gameInfo);
                    }
                }

                _lastResult = newResults;
                _channelId = broadcasterId;
                _lastCheck = DateTime.UtcNow;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error refreshing category for channelId {broadcasterId}", broadcasterId);
            }

            return _gameInfo;
        }

        Task<GameInfo> ITwitchCategoryProvider.FetchChannelInfo(string channelId)
        {
            return RefreshCategory(channelId);
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }
    }
}
