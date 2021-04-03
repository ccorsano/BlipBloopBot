using BlipBloopBot.Twitch.API;
using BotServiceGrain;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotServiceGrainInterface
{
    public class ChannelGrain : Grain, IChannelGrain
    {
        private readonly IPersistentState<ChannelState> _channelState;
        private readonly IPersistentState<ChannelBotSettingsState> _channelBotState;
        private readonly TwitchAPIClient _appClient;
        private readonly ILogger _logger;
        private string _channelId;

        private Task _botTask;
        private CancellationTokenSource _botCancellationSource;

        public ChannelGrain(
            [PersistentState("channel", "channelStore")] IPersistentState<ChannelState> channelState,
            [PersistentState("botsettings", "botSettingsStore")] IPersistentState<ChannelBotSettingsState> botSettingsState,
            TwitchAPIClient appClient,
            ILogger<ChannelGrain> logger)
        {
            _channelState = channelState;
            _channelBotState = botSettingsState;
            _appClient = appClient;
            _logger = logger;
        }

        public override Task OnActivateAsync()
        {
            _channelId = this.GetPrimaryKeyString();
            _logger.LogInformation("Activating channel grain {channelId}", _channelId);
            return base.OnActivateAsync();
        }

        public override Task OnDeactivateAsync()
        {
            _logger.LogInformation("Deactivating channel grain {channelId}", _channelId);
            return base.OnDeactivateAsync();
        }

        async Task<bool> IChannelGrain.SetBotActivation(bool isActive)
        {
            if (isActive ^ _channelBotState.State.IsActive)
            {
                if (isActive)
                {
                    await StartBot();
                }
                else
                {
                    await StopBot();
                }

                _channelBotState.State.IsActive = isActive;
                await _channelBotState.WriteStateAsync();
                return true;
            }
            return false;
        }

        async Task<HelixChannelInfo> IChannelGrain.GetChannelInfo()
        {
            var channelInfo = await _appClient.GetChannelInfoAsync(_channelId);
            return channelInfo;
        }

        Task IChannelGrain.HandleBotCommand()
        {
            throw new NotImplementedException();
        }

        private Task StartBot()
        {
            return Task.CompletedTask;
        }

        private Task StopBot()
        {
            return Task.CompletedTask;
        }
    }
}
