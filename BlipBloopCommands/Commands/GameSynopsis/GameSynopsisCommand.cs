using Conceptoire.Twitch.IRC;
using Conceptoire.Twitch.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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

        public async Task OnUpdateContext(IProcessorContext context)
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
        }

        public void OnMessage(ParsedIRCMessage message, Action<OutgoingMessage> sendResponse)
        {
            string msgId = null;
            foreach (var tag in message.ParseIRCTags())
            {
                if (tag.Key.SequenceEqual("id"))
                {
                    msgId = new string(tag.Value);
                    break;
                }
            }
            var reply = new OutgoingMessage
            {
                ReplyParentMessage = msgId,
                Message = _gameInfo?.Summary ?? "Not playing, we are just chilling at the moment !"
            };
            sendResponse(reply);
        }
    }
}
