using BlipBloopCommands.Commands.GameSynopsis;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Commands;
using Conceptoire.Twitch.IRC;
using Conceptoire.Twitch.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlipBloopBot
{
    public class BotHostedService : IHostedService
    {
        private readonly IHost _host;
        private readonly List<ChannelOptions> _channels;
        private readonly Dictionary<string, IMessageProcessor> _messageProcessors;
        private readonly ILogger _logger;

        private List<Task> _channelTasks;

        public BotHostedService(
            IHost host,
            IEnumerable<ChannelOptions> options,
            IEnumerable<CommandRegistration> commands,
            ILogger<BotHostedService> logger)
        {
            _host = host;
            _channels = options.ToList();
            _messageProcessors = commands.ToDictionary(c => c.Name, c => c.Processor());
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _channelTasks = _channels.Select(c => RunChannelAsync(c, cancellationToken)).ToList();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(_channelTasks);
        }

        public async Task RunChannelAsync(ChannelOptions options, CancellationToken cancellationToken)
        {
            try
            {
                using (var scope = _host.Services.CreateScope())
                {
                    var pollingCategoryProvider = scope.ServiceProvider.GetRequiredService<PollingTwitchCategoryProvider>();
                    pollingCategoryProvider.CheckAndSchedule(options.BroadcasterLogin);

                    var commandProcessors = options.Commands.Select(c => (Command: c.Key, Processor: _messageProcessors[c.Value.Type])).ToArray();

                    var twitchInfo = _host.Services.GetRequiredService<TwitchAPIClient>();
                    var broadcasterLogin = options.BroadcasterLogin.ToLowerInvariant();
                    var channelSearch = await twitchInfo.SearchChannelsAsync(broadcasterLogin);
                    var channelInfo = channelSearch.FirstOrDefault(c => c.BroadcasterLogin == broadcasterLogin);
                    if (channelInfo == null)
                    {
                        throw new Exception("Could not resolve broadcasterLogin");
                    }

                    var botContext = new ProcessorContext
                    {
                        ChannelId = channelInfo.Id,
                        ChannelName = channelInfo.DisplayName,
                        Language = channelInfo.BroadcasterLanguage,
                        CategoryId = channelInfo.GameId,
                    };

                    Func<IProcessorContext, Task> updatePollingContext = async (ctx) =>
                    {
                        await Task.WhenAll(commandProcessors.Select(processor => processor.Processor.OnUpdateContext(ctx)));
                    };
                    await updatePollingContext(botContext);

                    pollingCategoryProvider.OnUpdate += async (sender, gameInfo) =>
                    {
                        await updatePollingContext(new ProcessorContext
                        {
                            ChannelId = botContext.ChannelId,
                            ChannelName = botContext.ChannelName,
                            Language = string.IsNullOrEmpty(gameInfo.Language) ? botContext.Language : gameInfo.Language,
                            CategoryId = gameInfo.TwitchCategoryId,
                        });
                    };

                    var channelName = options.BroadcasterLogin.ToLowerInvariant();

                    var ircBuilder = scope.ServiceProvider.GetRequiredService<ITwitchChatClientBuilder>();
                    using (var ircClient = ircBuilder.Build())
                    {
                        await ircClient.ConnectAsync(cancellationToken);
                        await ircClient.JoinAsync(channelName, cancellationToken);
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await ircClient.ReceiveIRCMessage(botContext, null, cancellationToken);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error in channel listener");
            }
        }
    }
}
