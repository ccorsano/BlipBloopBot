using BotServiceGrain;
using BotServiceGrainInterface.Model;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Commands;
using Conceptoire.Twitch.IRC;
using Conceptoire.Twitch.Options;
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
        private readonly Dictionary<string, CommandRegistration> _registeredCommands;
        private readonly Dictionary<Guid, IMessageProcessor> _commandProcessors;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private string _channelId;
        private TwitchAPIClient _userClient;
        private HelixChannelInfo _channelInfo;

        private bool _commandsUpdate;
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
            _registeredCommands = commands.ToDictionary(c => c.Name, c => c);
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        public override async Task OnActivateAsync()
        {
            _channelId = this.GetPrimaryKeyString();
            _logger.LogInformation("Activating channel grain {channelId}", _channelId);

            if (!_channelBotState.RecordExists)
            {
                var defaultBotInfo = new BotAccountInfo
                {
                    UserId = _options.TokenInfo.UserId,
                    UserLogin = _options.TokenInfo.Login,
                };
                _channelBotState.State.AllowedBotAccounts.Add(defaultBotInfo);
                _channelBotState.State.Commands = new Dictionary<Guid, Conceptoire.Twitch.Commands.CommandOptions>
                {
                    { Guid.NewGuid(), new CommandOptions
                        {
                            Name = "*",
                            Type = "MessageTracer",
                        }
                    },
                    { Guid.NewGuid(), new CommandOptions
                        {
                            Name = "jeu",
                            Type = "GameSynopsis",
                            Parameters = new Dictionary<string, string>(),
                        }
                    }
                };
            }

            await base.OnActivateAsync();

            _channelInfo = await _appClient.GetChannelInfoAsync(_channelId);
            await OnChannelUpdate(_channelInfo);
        }

        private Task AddCommand(CommandOptions options)
        {
            _channelBotState.State.Commands.Add(Guid.NewGuid(), options);
            var command = _registeredCommands[options.Type].Processor();
            // WIP
            throw new NotImplementedException();
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

            var channelInfoTask = _userClient.GetChannelInfoAsync(_channelId);
            List<HelixChannelModerator> moderators = new List<HelixChannelModerator>();
            var editorsTask = _userClient.GetHelixChannelEditorsAsync(_channelId);
            await foreach (var moderator in _userClient.EnumerateChannelModeratorsAsync(_channelId))
            {
                moderators.Add(moderator);
            }
            _channelState.State.Editors = (await editorsTask).ToList();
            _channelState.State.Moderators = moderators.ToList();
            _channelInfo = await channelInfoTask;
        }

        Task<bool> IChannelGrain.IsBotActive()
        {
            return Task.FromResult(_channelBotState.State.IsActive);
        }

        private async Task<string> GetBotOAuthToken()
        {
            var activeBotInfo = _channelBotState.State.AllowedBotAccounts.FirstOrDefault(b => b.IsActive);
            if (activeBotInfo == null)
            {
                return _options.OAuthToken;
            }
            var botGrain = GrainFactory.GetGrain<IUserGrain>(activeBotInfo.UserId);
            return await botGrain.GetOAuthToken();
        }

        async Task<bool> IChannelGrain.SetBotActivation(bool isActive)
        {
            if (isActive ^ _channelBotState.State.IsActive)
            {
                if (isActive)
                {
                    await StartBot(await GetBotOAuthToken());
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

            await Task.WhenAll(commandProcessors.Select(processor => {
                var botContext = new ProcessorContext
                {
                    ChannelId = _channelId,
                    ChannelName = _channelInfo.BroadcasterName,
                    Language = _channelInfo.BroadcasterLanguage,
                    CategoryId = _channelInfo.GameId,
                    CommandOptions = _channelBotState.State.Commands[processor.Command]
                };
                return processor.Processor.OnUpdateContext(botContext);
            }));

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
                            if (_commandsUpdate)
                            {
                                lock(_botTask)
                                {
                                    commandProcessors = _channelBotState.State.Commands.Select(c => (Command: c.Key, Processor: _commands[c.Value.Type])).ToArray();
                                }
                            }
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

            var commandsUpdateTasks = _commands.Select(kvp => {
                var botContext = new ProcessorContext
                {
                    ChannelId = info.BroadcasterId,
                    ChannelName = info.BroadcasterName,
                    Language = info.BroadcasterLanguage,
                    CategoryId = info.GameId,
                    CommandOptions = _channelBotState.State.Commands[kvp.Key],
                };
                return kvp.Value.OnUpdateContext(botContext);
            });
            await Task.WhenAll(commandsUpdateTasks);
        }

        public Task<ChannelStaff> GetStaff()
        {
            return Task.FromResult(new ChannelStaff
            {
                Editors = _channelState.State.Editors?.ToArray() ?? new HelixChannelEditor[0],
                Moderators = _channelState.State.Moderators?.ToArray() ?? new HelixChannelModerator[0],
            });
        }

        public Task<CommandOptions[]> GetBotCommands()
        {
            return Task.FromResult(_channelBotState.State.Commands.Values.ToArray());
        }

        public Task UpdateBotCommands(CommandOptions[] commands)
        {
            lock(_botTask)
            {
                _channelBotState.State.Commands = commands.ToDictionary(c => c.Name, c => c);
            }
            _commandsUpdate = true;
            return Task.CompletedTask;
        }

        public Task<CommandMetadata[]> GetSupportedCommandTypes()
        {
            return Task.FromResult(_registeredCommands.Values.Select(v => v.Metadata).ToArray());
        }

        Task IChannelGrain.SetActiveBotAccount(string userId)
        {
            var accountInfo = _channelBotState.State.AllowedBotAccounts.FirstOrDefault(u => u.UserId == userId);
            if (accountInfo == null)
            {
                _logger.LogError("Trying to set a bot account not allowed on the channel {userId}", userId);
                throw new InvalidOperationException("Trying to set a bot account not allowed on the channel");
            }
            _logger.LogInformation("Setting {botAccountName}({botAccountId}) as active bot on channel {channelName}", accountInfo.UserLogin, accountInfo.UserId, _channelInfo.BroadcasterName);
            foreach (var bot in _channelBotState.State.AllowedBotAccounts)
            {
                bot.IsActive = bot.UserId == userId;
            }
            return Task.CompletedTask;
        }

        async Task IChannelGrain.AllowBotAccount(BotAccountInfo botAccount)
        {
            _channelBotState.State.AllowedBotAccounts.Add(botAccount);
            await _channelBotState.WriteStateAsync();
        }

        async Task IChannelGrain.DisallowBotAccount(string userId)
        {
            _channelBotState.State.AllowedBotAccounts.RemoveAll(bot => bot.UserId == userId);
            await _channelBotState.WriteStateAsync();
        }

        Task<BotAccountInfo[]> IChannelGrain.GetAllowedBotAccounts()
        {
            return Task.FromResult(_channelBotState.State.AllowedBotAccounts.ToArray());
        }
    }
}
