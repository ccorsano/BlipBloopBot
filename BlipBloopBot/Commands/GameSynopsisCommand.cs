using BlipBloopBot.Twitch.API;
using BlipBloopBot.Twitch.IRC;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlipBloopBot.Commands
{
    public class GameSynopsisCommand : IMessageProcessor
    {
        private readonly TwitchAPIClient _twitchAPIClient;
        private readonly IGDBClient _igdbClient;
        private readonly TwitchApplicationOptions _twitchOptions;
        private readonly ILogger _logger;

        private string _synopsis;

        public GameSynopsisCommand(
            TwitchAPIClient twitchAPIClient,
            IGDBClient igdbClient,
            IOptions<TwitchApplicationOptions> twitchOptions,
            ILogger<GameSynopsisCommand> logger)
        {
            _twitchAPIClient = twitchAPIClient;
            _igdbClient = igdbClient;
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

            var twitchExternalGameInfo = await _igdbClient.SearchExternalGame("uid", $"\"{channel.GameId}\"", IGDBExternalGameCategory.Twitch);
            if (twitchExternalGameInfo?.FirstOrDefault() != null)
            {
                var fullGameInfo = await _igdbClient.GetGameByIdAsync(twitchExternalGameInfo.First().Game);
                if (fullGameInfo != null)
                {
                    _synopsis = fullGameInfo.Summary;
                    _logger.LogWarning($"Playing {fullGameInfo.Name}: {fullGameInfo.Summary}");
                }
            }
        }

        public void OnMessage(ParsedIRCMessage message, Action<string> sendResponse)
        {
            sendResponse(_synopsis ?? "Not playing, we are just chilling at the moment !");
        }
    }
}
