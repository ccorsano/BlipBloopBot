using Conceptoire.Twitch.Commands;
using Conceptoire.Twitch.Options;
using Conceptoire.Twitch.API;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotServiceGrain
{
    public class LoadConfigurationStartupTask : IStartupTask
    {
        private readonly IGrainFactory _grainFactory;
        private TwitchAPIClient _twitchClient;
        private readonly IEnumerable<ChannelOptions> _channels;
        private readonly IEnumerable<CommandRegistration> _commands;
        private readonly ILogger _logger;

        public LoadConfigurationStartupTask(
            IGrainFactory grainFactory,
            TwitchAPIClient twitchClient,
            IEnumerable<ChannelOptions> options,
            IEnumerable<CommandRegistration> commands,
            ILogger<LoadConfigurationStartupTask> logger)
        {
            _grainFactory = grainFactory;
            _twitchClient = twitchClient;
            _channels = options.ToList();
            _commands = commands.ToList();
            _logger = logger;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            foreach(var channel in _channels)
            {
                var channelResult = await _twitchClient.SearchChannelsAsync(channel.BroadcasterLogin);
                if (! channelResult.Any())
                {
                    _logger.LogError("Could not find channel {broadcasterLogin}", channel.BroadcasterLogin);
                    continue;
                }
                var grain = _grainFactory.GetGrain<IChannelGrain>(channelResult.First().Id);
                await grain.GetChannelInfo();
            }
        }
    }
}
