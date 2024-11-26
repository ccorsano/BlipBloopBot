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
using Conceptoire.Twitch.Options;
using Microsoft.Extensions.Hosting;
using Conceptoire.Twitch.Steam;
using Conceptoire.Twitch.Constants;

namespace BlipBloopCommands.Commands.GameSynopsis
{
    public class PollingTwitchCategoryProvider : ITwitchCategoryProvider, IDisposable, IHostedService
    {
        private readonly TwitchAPIClient _twitchAPIClient;
        private readonly IGDBClient _igdbClient;
        private readonly SteamStoreClient _steamStoreClient;
        private readonly IGameLocalizationStore _gameLocalization;
        private readonly TwitchApplicationOptions _twitchOptions;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private string _broacasterLogin;
        private HelixChannelSearchResult _lastResult;
        private HelixChannelInfo _lastInfo;
        private GameInfo _gameInfo;

        private DateTime _lastCheck;
        private TaskCompletionSource _broadcasterLoginSet = new TaskCompletionSource();

        public event EventHandler<GameInfo> OnUpdate;

        public PollingTwitchCategoryProvider(
            TwitchAPIClient twitchAPIClient,
            IGDBClient IGDBClient,
            SteamStoreClient steamStoreClient,
            IGameLocalizationStore gameLocalizationStore,
            IOptions<TwitchApplicationOptions> twitchOptions,
            ILogger<PollingTwitchCategoryProvider> logger)
        {
            _twitchAPIClient = twitchAPIClient;
            _igdbClient = IGDBClient;
            _gameLocalization = gameLocalizationStore;
            _twitchOptions = twitchOptions.Value;
            _logger = logger;
        }

        public bool CheckAndSchedule(string broacasterLogin)
        {
            if (broacasterLogin != _broacasterLogin)
            {
                _lastCheck = DateTime.MinValue;
                _broacasterLogin = broacasterLogin;
                _broadcasterLoginSet.TrySetResult();
                return false;
            }

            return true;
        }

        public async Task<GameInfo> RefreshCategory(string broadcasterId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting category refresh for channelId {broadcasterId}", broadcasterId);

            try
            {
                var results = await _twitchAPIClient.SearchChannelsAsync(broadcasterId, cancellationToken);
                var newResults = results.First(c => c.BroadcasterLogin == broadcasterId);

                if (newResults.GameId != _lastResult?.GameId)
                {
                    _lastInfo = await _twitchAPIClient.GetChannelInfoAsync(newResults.Id, cancellationToken);
                    _gameInfo = await _gameLocalization.ResolveLocalizedGameInfoAsync(newResults.BroadcasterLanguage, newResults.GameId, cancellationToken);

                    if (_gameInfo != null && OnUpdate != null)
                    {
                        OnUpdate(this, _gameInfo);
                    }
                }

                _lastResult = newResults;
                _broacasterLogin = broadcasterId;
                _lastCheck = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing category for channelId {broadcasterId}", broadcasterId);
            }

            return _gameInfo;
        }

        async Task<GameInfo> ITwitchCategoryProvider.FetchChannelInfo(string categoryId, string language, CancellationToken cancellationToken)
        {
            if (_broacasterLogin != null && (_gameInfo?.Language != language || _gameInfo?.TwitchCategoryId != categoryId))
            {
                await RefreshCategory(_broacasterLogin, cancellationToken);
            }
            return await _gameLocalization.ResolveLocalizedGameInfoAsync(language, categoryId);
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var unawaited = Task.Run(async () =>
            {
                await _broadcasterLoginSet.Task;
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await RefreshCategory(_broacasterLogin, cancellationToken);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}
