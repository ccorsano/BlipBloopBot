using BlipBloopBot.Commands;
using BlipBloopBot.Options;
using BlipBloopBot.Twitch.API;
using BlipBloopBot.Twitch.IRC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    var commandProcessors = options.Commands.Select(c => (Command: c.Key, Processor: _messageProcessors[c.Value.Type])).ToArray();
                    await Task.WhenAll(commandProcessors.Select(processor => processor.Processor.Init(options.BroadcasterLogin.ToLowerInvariant())));
                    var channelName = options.BroadcasterLogin.ToLowerInvariant();

                    var ircBuilder = scope.ServiceProvider.GetRequiredService<ITwitchChatClientBuilder>();
                    using (var ircClient = ircBuilder.Build())
                    {
                        await ircClient.ConnectAsync(cancellationToken);
                        await ircClient.JoinAsync(channelName, cancellationToken);
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await ircClient.ReceiveIRCMessage(commandProcessors, cancellationToken);
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
