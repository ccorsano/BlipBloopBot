using BlipBloopBot.Model;
using BlipBloopBot.Storage;
using BlipBloopBot.Twitch;
using BlipBloopBot.Twitch.API;
using BlipBloopBot.Twitch.IRC;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlipBloopBot.Commands
{
    public class GameSynopsisCommand : IMessageProcessor
    {
        private readonly TwitchAPIClient _twitchAPIClient;
        private readonly IGDBClient _igdbClient;
        private readonly IGameLocalizationStore _localizationStore;
        private readonly TwitchApplicationOptions _twitchOptions;
        private readonly ILogger _logger;

        private GameInfo _gameInfo;

        public GameSynopsisCommand(
            TwitchAPIClient twitchAPIClient,
            IGDBClient igdbClient,
            IGameLocalizationStore localizationStore,
            IOptions<TwitchApplicationOptions> twitchOptions,
            ILogger<GameSynopsisCommand> logger)
        {
            _twitchAPIClient = twitchAPIClient;
            _igdbClient = igdbClient;
            _localizationStore = localizationStore;
            _twitchOptions = twitchOptions.Value;
            _logger = logger;
        }

        public async Task Init(string broadcasterLogin)
        {
            await _twitchAPIClient.AuthenticateAsync(_twitchOptions.ClientId, _twitchOptions.ClientSecret);
            await _igdbClient.AuthenticateAsync(_twitchOptions.ClientId, _twitchOptions.ClientSecret);

            var results = await _twitchAPIClient.SearchChannelsAsync(broadcasterLogin);
            var channelStatus = results.First(c => c.BroadcasterLogin == broadcasterLogin);
            var channel = await _twitchAPIClient.GetChannelInfoAsync(channelStatus.Id);
            var channelName = channel.BroadcasterName.ToLowerInvariant();

            if (!channelStatus.IsLive)
            {
                _logger.LogWarning($"{channelStatus.DisplayName} is not live");
            }
            else
            {
                _logger.LogWarning($"Connecting to {channel.BroadcasterName}, currently live");
            }

            _gameInfo = await _localizationStore.ResolveLocalizedGameInfo(channelStatus.BroadcasterLanguage, channelStatus.GameId);
        }

        public void OnMessage(ParsedIRCMessage message, Action<string> sendResponse)
        {
            sendResponse(_gameInfo?.Summary ?? "Not playing, we are just chilling at the moment !");
        }
    }
}
