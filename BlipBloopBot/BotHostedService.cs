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
        private readonly IEnumerable<IMessageProcessor> _messageProcessors;
        private readonly ILogger _logger;

        private List<Task> _channelTasks;

        public BotHostedService(
            IHost host,
            IEnumerable<ChannelOptions> options,
            IEnumerable<IMessageProcessor> processors,
            ILogger<BotHostedService> logger)
        {
            _host = host;
            _channels = options.ToList();
            _messageProcessors = processors;
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
                    using (var apiClient = scope.ServiceProvider.GetRequiredService<TwitchAPIClient>())
                    {
                        var appConfig = scope.ServiceProvider.GetRequiredService<IOptions<TwitchApplicationOptions>>();
                        await apiClient.AuthenticateAsync(appConfig.Value.ClientId, appConfig.Value.ClientSecret);

                        var results = await apiClient.SearchChannelsAsync(options.BroadcasterLogin);
                        var channelStatus = results.First(c => c.BroadcasterLogin == options.BroadcasterLogin);
                        var channel = await apiClient.GetChannelInfoAsync(channelStatus.Id);
                        var channelName = channel.BroadcasterName.ToLowerInvariant();

                        if (!channelStatus.IsLive)
                        {
                            _logger.LogWarning($"{channelStatus.DisplayName} is not live");
                        }
                        else
                        {
                            _logger.LogWarning($"Connecting to {channel.BroadcasterName}, currently live");
                        }

                        using (var ircClient = scope.ServiceProvider.GetRequiredService<TwitchChatClient>())
                        {
                            await ircClient.ConnectAsync(cancellationToken);
                            await ircClient.SendCommandAsync("JOIN", $"#{channelName}", cancellationToken);
                            while (!cancellationToken.IsCancellationRequested)
                            {
                                await ircClient.ReceiveIRCMessage(cancellationToken);
                            }
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
