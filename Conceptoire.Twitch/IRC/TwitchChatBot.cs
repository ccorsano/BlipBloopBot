using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Authentication;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public class TwitchChatBot : IHostedService
    {
        private readonly IAuthenticated _authenticated;
        private TwitchAPIClient _twitchAPIClient;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private readonly TwitchChatClientOptions _options;
        private readonly Dictionary<Guid, IMessageProcessor> _commandProcessors;

        private bool _commandsUpdate;
        private Task _botTask;
        private CancellationTokenSource _botCancellationSource;

        public TwitchChatBot(IAuthenticated authenticated, TwitchAPIClient twitchAPIClient, ILoggerFactory loggerFactory, ILogger<TwitchChatBot> logger)
        {
            _authenticated = authenticated;
            _twitchAPIClient = twitchAPIClient;
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken externalCancellationToken)
        {
            var botChatClientBuilder = TwitchChatClientBuilder.Create()
                .WithAuthenticatedUser(_authenticated)
                .WithLoggerFactory(_loggerFactory);
            var orleansTaskScheduler = TaskScheduler.Current;
            

            _botCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken);
            var cancellationToken = _botCancellationSource.Token;
            //var commandProcessors = _channelBotState.State.Commands.Select(c => (Command: c.Key, Processor: _commands[c.Value.Type])).ToArray();

            //await Task.WhenAll(commandProcessors.Select(processor => {
            //    var botContext = new ProcessorContext
            //    {
            //        ChannelId = _channelId,
            //        ChannelName = _channelInfo.BroadcasterName,
            //        Language = _channelInfo.BroadcasterLanguage,
            //        CategoryId = _channelInfo.GameId,
            //        CommandOptions = _channelBotState.State.Commands[processor.Command]
            //    };
            //    return processor.Processor.OnUpdateContext(botContext);
            //}));

            _botTask = Task.Run(async () =>
            {
                try
                {
                    var channelName = _channelInfo.BroadcasterName.ToLowerInvariant();

                    using (var ircClient = botChatClientBuilder.Build())
                    {
                        await ircClient.ConnectAsync(cancellationToken);
                        await ircClient.JoinAsync(channelName, cancellationToken);
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await ircClient.ReceiveIRCMessage(commandProcessors, cancellationToken);
                            //if (_commandsUpdate)
                            //{
                            //    lock (_botTask)
                            //    {
                            //        commandProcessors = _channelBotState.State.Commands.Select(c => (Command: c.Key, Processor: _commands[c.Value.Type])).ToArray();
                            //    }
                            //}
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in channel listener");
                }
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
