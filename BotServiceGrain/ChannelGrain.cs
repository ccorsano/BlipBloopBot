using BotServiceGrain;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Commands;
using Conceptoire.Twitch.IRC;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotServiceGrainInterface
{
    public class ChannelGrain : Grain, IChannelGrain
    {
        private readonly IPersistentState<ChannelState> _channelState;
        private readonly IPersistentState<ChannelBotSettingsState> _channelBotState;
        private readonly TwitchAPIClient _appClient;
        private readonly TwitchChatClientOptions _options;
        private readonly Dictionary<string, IMessageProcessor> _commands;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private string _channelId;
        private TwitchAPIClient _userClient;
        private HelixChannelInfo _channelInfo;

        private Task _botTask;
        private CancellationTokenSource _botCancellationSource;

        public ChannelGrain(
            [PersistentState("channel", "channelStore")] IPersistentState<ChannelState> channelState,
            [PersistentState("botsettings", "botSettingsStore")] IPersistentState<ChannelBotSettingsState> botSettingsState,
            TwitchAPIClient appClient,
            IOptions<TwitchChatClientOptions> botOptions,
            IEnumerable<CommandRegistration> commands,
            ILoggerFactory loggerFactory,
            ILogger<ChannelGrain> logger)
        {
            _channelState = channelState;
            _channelBotState = botSettingsState;
            _appClient = appClient;
            _options = botOptions.Value;
            _commands = commands.ToDictionary(c => c.Name, c => c.Processor());
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        public override async Task OnActivateAsync()
        {
            _channelId = this.GetPrimaryKeyString();
            _logger.LogInformation("Activating channel grain {channelId}", _channelId);

            if (!_channelBotState.RecordExists)
            {
                _channelBotState.State.Commands = new Dictionary<string, Conceptoire.Twitch.Options.CommandOptions>
                {
                    { "*", new Conceptoire.Twitch.Options.CommandOptions
                        {
                            Type = "MessageTracer"
                        }
                    }
                };
            }

            await base.OnActivateAsync();

            _channelInfo = await _appClient.GetChannelInfoAsync(_channelId);
            await OnChannelUpdate(_channelInfo);
        }

        public override Task OnDeactivateAsync()
        {
            _logger.LogInformation("Deactivating channel grain {channelId}", _channelId);
            return base.OnDeactivateAsync();
        }

        async Task IChannelGrain.Activate(string userToken)
        {
            var userAuthenticated = Conceptoire.Twitch.Twitch.Authenticate()
                .FromOAuthToken(userToken)
                .Build();
            var userClient = TwitchAPIClient.CreateFromBase(_appClient, userAuthenticated);
            var validated = await userClient.ValidateToken();
            if (validated == null || validated.UserId != _channelId || validated.ExpiresIn == 0)
            {
                throw new ArgumentException("Could not validate token");
            }
            _userClient = userClient;

            List<HelixChannelModerator> moderators = new List<HelixChannelModerator>();
            var editorsTask = _userClient.GetHelixChannelEditorsAsync(_channelId);
            await foreach(var moderator in _userClient.EnumerateChannelModeratorsAsync(_channelId))
            {
                moderators.Add(moderator);
            }
            _channelState.State.Editors = (await editorsTask).ToArray();
            _channelState.State.Moderators = moderators.ToArray();
        }

        async Task<bool> IChannelGrain.SetBotActivation(bool isActive)
        {
            if (isActive ^ _channelBotState.State.IsActive)
            {
                if (isActive)
                {
                    await StartBot(_options.OAuthToken);
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

        Task<HelixChannelInfo> IChannelGrain.GetChannelInfo()
        {
            return Task.FromResult(_channelInfo);
        }

        Task IChannelGrain.HandleBotCommand()
        {
            throw new NotImplementedException();
        }

        private async Task StartBot(string oauthToken)
        {
            var botChatClientBuilder = TwitchChatClientBuilder.Create()
                .WithOAuthToken(oauthToken)
                .WithLoggerFactory(_loggerFactory);
            var orleansTaskScheduler = TaskScheduler.Current;

            _botCancellationSource = new CancellationTokenSource();
            var cancellationToken = _botCancellationSource.Token;
            var commandProcessors = _channelBotState.State.Commands.Select(c => (Command: c.Key, Processor: _commands[c.Value.Type])).ToArray();
            await Task.WhenAll(commandProcessors.Select(processor => processor.Processor.Init(_channelInfo.BroadcasterName)));

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
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in channel listener");
                }
            });
        }

        private Task StopBot()
        {
            _botCancellationSource.Cancel();
            return Task.CompletedTask;
        }

        public async Task OnChannelUpdate(HelixChannelInfo info)
        {
            _channelState.State.LastCategoryId = info.GameId;
            _channelState.State.LastCategoryName = info.GameName;
            _channelState.State.LastLanguage = info.BroadcasterLanguage;
            _channelState.State.LastTitle = info.Title;
            await _channelState.WriteStateAsync();
        }
    }
}
