using BotServiceGrain;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Threading.Tasks;

namespace BotServiceGrainInterface
{
    public class ChannelGrain : Grain, IChannelGrain
    {
        private readonly ILogger _logger;

        public ChannelGrain(ILogger<ChannelGrain> logger)
        {
            _logger = logger;
        }

        public override Task OnActivateAsync()
        {
            return base.OnActivateAsync();
        }

        public override Task OnDeactivateAsync()
        {
            return base.OnDeactivateAsync();
        }

        public Task<string> GetChannelInfo()
        {
            return Task.FromResult("blah");
        }

        public Task HandleBotCommand()
        {
            throw new NotImplementedException();
        }
    }
}
