﻿using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Authentication;
using Conceptoire.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public class TwitchChatBot : IHostedService
    {
        private readonly IBotAuthenticated _authenticated;
        private TwitchAPIClient _twitchAPIClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<Guid, IMessageProcessor> _commandProcessors = new ConcurrentDictionary<Guid, IMessageProcessor>();

        private TaskCompletionSource<string> _channelIdCompletionSource = new TaskCompletionSource<string>();

        private bool _commandsUpdate;
        private Task _botTask;
        private CancellationTokenSource _botCancellationSource;
        private IProcessorContext _currentContext;

        public TwitchChatBot(IBotAuthenticated authenticated, TwitchAPIClient twitchAPIClient, IServiceProvider serviceProvider, ILogger<TwitchChatBot> logger)
        {
            _authenticated = authenticated;
            _twitchAPIClient = twitchAPIClient;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public bool SetChannel(string channelId)
        {
            return _channelIdCompletionSource.TrySetResult(channelId);
        }

        public Task UpdateContext(IProcessorContext context)
        {
            _currentContext = context;

            return Task.WhenAll(_commandProcessors.Values.Select(c => c.OnUpdateContext(context)));
        }

        public Task RegisterMessageProcessor<TMessageProcessor>(Guid commandId, CommandOptions options = null) where TMessageProcessor : IMessageProcessor
         => RegisterMessageProcessor(commandId, typeof(TMessageProcessor), options);

        public Task RegisterMessageProcessor<TMessageProcessor>(CommandOptions options = null) where TMessageProcessor : IMessageProcessor
         => RegisterMessageProcessor(typeof(TMessageProcessor), options);

        public Task<IMessageProcessor> RegisterMessageProcessor(Type messageProcessorType, CommandOptions options = null)
        {
            var newCommandId = options?.Id ?? Guid.NewGuid();
            return RegisterMessageProcessor(newCommandId, messageProcessorType, options);
        }

        public async Task<IMessageProcessor> RegisterMessageProcessor(Guid commandId, Type messageProcessorType, CommandOptions options = null)
        {
            var processor = _serviceProvider.GetRequiredService(messageProcessorType) as IMessageProcessor;

            await processor.LoadSettings(commandId, await _channelIdCompletionSource.Task, options);
            var initTask = Task.Run(async () =>
            {
                if (_currentContext != null)
                {
                    await processor.OnUpdateContext(_currentContext);
                }
                if (!_commandProcessors.TryAdd(commandId, processor))
                {
                    throw new InvalidOperationException("Could not add command, unexpectedly. Id collision ??");
                }
            });

            return processor;
        }

        public Task<bool> RemoveMessageProcessor(Guid processorId)
        {
            return Task.FromResult(_commandProcessors.TryRemove(processorId, out IMessageProcessor processor));
        }

        public Task StartAsync(CancellationToken externalCancellationToken)
        {
            var botChatClientBuilder = TwitchChatClientBuilder.Create()
                .WithAuthenticatedUser(_authenticated)
                .WithLoggerFactory(_serviceProvider.GetRequiredService<ILoggerFactory>());
            var orleansTaskScheduler = TaskScheduler.Current;
            
            _botCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken);
            var cancellationToken = _botCancellationSource.Token;

            _botTask = Task.Run(async () =>
            {
                try
                {
                    var channelId = await _channelIdCompletionSource.Task;
                    var channelInfo = await _twitchAPIClient.GetChannelInfoAsync(channelId, cancellationToken);

                    var channelName = channelInfo.BroadcasterName.ToLowerInvariant();

                    var botContext = new ProcessorContext
                    {
                        ChannelId = channelId,
                        ChannelName = channelInfo.BroadcasterName,
                        Language = channelInfo.BroadcasterLanguage,
                        CategoryId = channelInfo.GameId,
                    };

                    using (var ircClient = botChatClientBuilder.Build())
                    {
                        await ircClient.ConnectAsync(cancellationToken);
                        await ircClient.JoinAsync(channelName, cancellationToken);
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await ircClient.ReceiveIRCMessage(botContext, _commandProcessors.Values, cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in channel listener");
                }
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (! _botTask.IsCompleted)
            {
                _botCancellationSource.Cancel();
            }
            return _botTask;
        }
    }
}
