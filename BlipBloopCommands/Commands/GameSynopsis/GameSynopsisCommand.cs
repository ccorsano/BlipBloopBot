using Conceptoire.Twitch.Extensions;
using Conceptoire.Twitch.IRC;
using Conceptoire.Twitch.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlipBloopCommands.Commands.GameSynopsis
{
    public class GameSynopsisCommand : BotCommandBase
    {
        private readonly ITwitchCategoryProvider _twitchCategoryProvider;
        private readonly ILogger _logger;

        private bool _asReply;
        private string _channelId;
        private GameInfo _gameInfo;

        private GameSynopsisCommandSettings _settings;

        private string[] _aliases;

        public GameSynopsisCommand(
            ITwitchCategoryProvider twitchProvider,
            ILogger<GameSynopsisCommand> logger)
        {
            _twitchCategoryProvider = twitchProvider;
            _logger = logger;
        }

        public override Task<IProcessorSettings> CreateSettings(Guid processorId)
        {
            _settings = new GameSynopsisCommandSettings();
            return Task.FromResult<IProcessorSettings>(_settings);
        }

        public override Task OnChangeSettings(IProcessorSettings settings)
        {
            _settings = settings as GameSynopsisCommandSettings;
            return Task.CompletedTask;
        }

        public override bool CanHandleMessage(in ParsedIRCMessage message)
        {
            foreach (var botCommand in message.Trailing.ParseBotCommands('!'))
            {
                foreach(var alias in _aliases)
                {
                    if (alias == botCommand)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override async Task OnUpdateContext(IProcessorContext context)
        {
            _logger.LogWarning("Received channel update for {channelId}: Category={categoryId} Lang={language}", context.ChannelId, context.CategoryId, context.Language);

            _channelId = context.ChannelId;

            _gameInfo = await _twitchCategoryProvider.FetchChannelInfo(context.CategoryId, context.Language);

            if (_gameInfo == null)
            {
                _logger.LogWarning($"{context.ChannelName} is not live");
            }
            else
            {
                _logger.LogWarning($"Connecting to {context.ChannelName}, currently live");
            }

            _twitchCategoryProvider.OnUpdate += (sender, gameInfo) =>
            {
                _gameInfo = gameInfo;
            };

            _asReply = _settings.AsReply;
        }

        public override void OnMessage(in ParsedIRCMessage message, Action<OutgoingMessage> sendResponse)
        {
            string msgId = _asReply ? message.GetMessageIdTag() : null;
            var reply = new OutgoingMessage
            {
                ReplyParentMessage = msgId,
                Message = _gameInfo?.Summary ?? "Not playing, we are just chilling at the moment !"
            };
            sendResponse(reply);
        }
    }
}
