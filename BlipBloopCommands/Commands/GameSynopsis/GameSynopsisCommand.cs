using BlipBloopBot.Model;
using BlipBloopBot.Storage;
using BlipBloopBot.Twitch;
using BlipBloopBot.Twitch.API;
using BlipBloopBot.Twitch.IRC;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlipBloopCommands.Commands.GameSynopsis
{
    public class GameSynopsisCommand : IMessageProcessor
    {
        private readonly ITwitchCategoryProvider _twitchCategoryProvider;
        private readonly ILogger _logger;

        private string _channelId;
        private GameInfo _gameInfo;

        public GameSynopsisCommand(
            ITwitchCategoryProvider twitchProvider,
            ILogger<GameSynopsisCommand> logger)
        {
            _twitchCategoryProvider = twitchProvider;
            _logger = logger;
        }

        public async Task Init(string broadcasterLogin)
        {
            _channelId = broadcasterLogin;

            _gameInfo = await _twitchCategoryProvider.FetchChannelInfo(broadcasterLogin);

            if (_gameInfo == null)
            {
                _logger.LogWarning($"{broadcasterLogin} is not live");
            }
            else
            {
                _logger.LogWarning($"Connecting to {broadcasterLogin}, currently live");
            }

            _twitchCategoryProvider.OnUpdate += (sender, gameInfo) =>
            {
                _gameInfo = gameInfo;
            };
        }

        public void OnMessage(ParsedIRCMessage message, Action<string> sendResponse)
        {
            sendResponse(_gameInfo?.Summary ?? "Not playing, we are just chilling at the moment !");
        }
    }
}
