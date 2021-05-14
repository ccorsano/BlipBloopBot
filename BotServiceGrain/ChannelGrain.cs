using BotServiceGrain;
using BotServiceGrainInterface.Model;
using Conceptoire.Twitch;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Commands;
using Conceptoire.Twitch.IRC;
using Conceptoire.Twitch.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Conceptoire.Twitch.Constants.TwitchConstants;

namespace BotServiceGrainInterface
{
    public class ChannelGrain : Grain, IChannelGrain
    {
        private readonly IPersistentState<ChannelState> _channelState;
        private readonly IPersistentState<ChannelBotSettingsState> _channelBotState;
        private readonly IPersistentState<CategoryDescriptionState> _categoriesState;
        private readonly TwitchAPIClient _appClient;
        private readonly TwitchChatClientOptions _options;
        private readonly TwitchApplicationOptions _twitchOptions;
        private readonly Dictionary<string, CommandRegistration> _registeredCommands;
        private readonly ILogger _logger;
        private string _channelId;
        private TwitchAPIClient _userClient;
        private HelixChannelInfo _channelInfo;
        private Dictionary<Guid, IProcessorSettings> _commandProcessors;
        private TwitchChatBot _chatBot;

        private bool _commandsUpdate;
        private Task _botTask;
        private CancellationTokenSource _botCancellationSource;

        public ChannelGrain(
            [PersistentState("channel", "channelStore")] IPersistentState<ChannelState> channelState,
            [PersistentState("botsettings", "botSettingsStore")] IPersistentState<ChannelBotSettingsState> botSettingsState,
            [PersistentState("customcategories", "customCategoriesStore")] IPersistentState<CategoryDescriptionState> customCategoriesState,
            TwitchAPIClient appClient,
            IOptions<TwitchChatClientOptions> botOptions,
            IOptions<TwitchApplicationOptions> appOptions,
            IEnumerable<CommandRegistration> commands,
            ILogger<ChannelGrain> logger)
        {
            _channelState = channelState;
            _channelBotState = botSettingsState;
            _categoriesState = customCategoriesState;
            _appClient = appClient;
            _options = botOptions.Value;
            _twitchOptions = appOptions.Value;
            _registeredCommands = commands.ToDictionary(c => c.Name, c => c);
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
                _channelBotState.State.Commands = new Dictionary<Guid, Conceptoire.Twitch.Commands.CommandOptions>();
            }
            else
            {
                // Temps: fix existing ids
                foreach((var id, var command) in _channelBotState.State.Commands)
                {
                    command.Id = id;
                    if (command.Type == "MessageTracer")
                    {
                        command.Type = "Logger";
                    }
                }
                // Activate bot if it is supposed to be running
                if (_channelBotState.State.IsActive)
                {
                    await RegisterEventSubSubscriptions(CancellationToken.None);
                    await StartBot(_channelState.State.BroadcasterToken);
                }
            }

            await base.OnActivateAsync();

            _channelInfo = await _appClient.GetChannelInfoAsync(_channelId);
            await OnChannelUpdate(_channelInfo);
        }

        public async Task AddCommand(CommandOptions options)
        {
            options.Id = Guid.NewGuid();
            _logger.LogInformation("Adding bot command {commandId} ({commandName} - {commandType})", options.Id, options.Name, options.Type);
            _channelBotState.State.Commands.Add(options.Id, options);
            var command = _registeredCommands[options.Type].Processor();

            var registration = _registeredCommands[options.Type];
            await _chatBot.RegisterMessageProcessor(registration.ProcessorType, options);
        }

        public async Task DeleteCommand(Guid commandId)
        {
            var command = _channelBotState.State.Commands.GetValueOrDefault(commandId);
            _logger.LogInformation("Removing bot command {commandId} ({commandName} - )", commandId, command?.Name, command?.Type);
            _channelBotState.State.Commands.Remove(commandId);
            await _chatBot.RemoveMessageProcessor(commandId);
        }

        public override Task OnDeactivateAsync()
        {
            _logger.LogInformation("Deactivating channel grain {channelId}", _channelId);
            return base.OnDeactivateAsync();
        }

        async Task IChannelGrain.Activate(string userToken)
        {
            var userAuthenticated = Twitch.Authenticate()
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
            _channelState.State.BroadcasterToken = userToken;
            _channelState.State.Editors = (await editorsTask).ToList();
            _channelState.State.Moderators = moderators.ToList();
            await _channelState.WriteStateAsync();

            _channelInfo = await channelInfoTask;
            await RegisterEventSubSubscriptions(CancellationToken.None);
        }

        private async Task RegisterEventSubSubscriptions(CancellationToken cancellationToken)
        {
            if (! Uri.TryCreate(_twitchOptions?.EventSub?.CallbackUrl, UriKind.Absolute, out Uri callbackUri))
            {
                _logger.LogError("No valid twitch:EventSub:BaseUrl provided: Cannot register EventSub subscriptions for {channelId}", _channelId);
                return;
            }

            if (string.IsNullOrEmpty(_twitchOptions?.EventSub?.WebHookSecret))
            {
                _logger.LogError("No twitch:EventSub:WebHookSecret provided: Cannot register EventSub subscriptions for {channelId}", _channelId);
                return;
            }

            bool isChannelUpdateRegistered = false;
            await foreach (var subscription in _appClient.EnumerateEventSubSubscriptions(EventSubStatus.Enabled, cancellationToken))
            {
                isChannelUpdateRegistered |= subscription.Type == EventSubTypes.ChannelUpdate;
            }

            if (! isChannelUpdateRegistered)
            {
                await _appClient.CreateEventSubChannelUpdateSubscription(_channelId, callbackUri, _twitchOptions.EventSub.WebHookSecret, cancellationToken);
            }

        }

        Task<bool> IChannelGrain.IsBotActive()
        {
            return Task.FromResult(_channelBotState.State.IsActive && _chatBot != null);
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

        private async Task StartBot(string oauthToken)
        {
            var botChatAuthenticated = Twitch.AuthenticateBot()
                .FromOAuthToken(oauthToken)
                .Build();
            _chatBot = new TwitchChatBot(botChatAuthenticated, _appClient, ServiceProvider, ServiceProvider.GetRequiredService<ILogger<TwitchChatBot>>());

            var orleansTaskScheduler = TaskScheduler.Current;

            _botCancellationSource = new CancellationTokenSource();
            var cancellationToken = _botCancellationSource.Token;
            _botTask = Task.Run(() => _chatBot.StartAsync(cancellationToken));

            _chatBot.SetChannel(_channelId);

            foreach ((var key, var processorInfo) in _channelBotState.State.Commands)
            {
                var registration = _registeredCommands[processorInfo.Type];
                await _chatBot.RegisterMessageProcessor(registration.ProcessorType, processorInfo);
            }
        }

        private Task StopBot()
        {
            return _chatBot.StopAsync(CancellationToken.None);
        }

        public async Task OnChannelUpdate(HelixChannelInfo info)
        {
            _channelState.State.LastCategoryId = info.GameId;
            _channelState.State.LastCategoryName = info.GameName;
            _channelState.State.LastLanguage = info.BroadcasterLanguage;
            _channelState.State.LastTitle = info.Title;
            await _channelState.WriteStateAsync();

            if (_chatBot == null)
            {
                return;
            }

            var botContext = new ProcessorContext
            {
                ChannelId = info.BroadcasterId,
                ChannelName = info.BroadcasterName,
                Language = info.BroadcasterLanguage,
                CategoryId = info.GameId,
            };
            var key = new CategoryKey { TwitchCategoryId = info.GameId, Locale = info.BroadcasterLanguage };
            if (_categoriesState.State.Descriptions.TryGetValue(key, out var customDescription))
            {
                botContext.CustomCategoryDescription = customDescription.Description;
            }
            else
            {
                botContext.CustomCategoryDescription = null;
            }
            await _chatBot.UpdateContext(botContext);
        }

        Task<ChannelStaff> IChannelGrain.GetStaff()
        {
            return Task.FromResult(new ChannelStaff
            {
                Editors = _channelState.State.Editors?.ToArray() ?? new HelixChannelEditor[0],
                Moderators = _channelState.State.Moderators?.ToArray() ?? new HelixChannelModerator[0],
            });
        }

        Task<CommandOptions[]> IChannelGrain.GetBotCommands()
        {
            return Task.FromResult(_channelBotState.State.Commands.Values.ToArray());
        }

        Task IChannelGrain.UpdateBotCommands(CommandOptions[] commands)
        {
            lock(_botTask)
            {
                //_channelBotState.State.Commands = commands.ToDictionary(c => c.Name, c => c);
            }
            _commandsUpdate = true;
            return Task.CompletedTask;
        }

        Task<CommandMetadata[]> IChannelGrain.GetSupportedCommandTypes()
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

        public async Task ClearCustomizedCategoryDescription(string twitchCategory, string locale)
        {
            var key = new CategoryKey { TwitchCategoryId = twitchCategory, Locale = locale };
            if (_categoriesState.State.Descriptions.TryGetValue(key, out var category))
            {
                _categoriesState.State.Descriptions.Remove(key);
            }
            await _categoriesState.WriteStateAsync();

            if (twitchCategory == _channelInfo.GameId && locale == _channelInfo.BroadcasterLanguage)
            {
                await OnChannelUpdate(_channelInfo);
            }
        }

        public async Task SetCustomizedCategoryDescription(CustomCategoryDescription categoryDescription)
        {
            var key = new CategoryKey { TwitchCategoryId = categoryDescription.TwitchCategoryId, Locale = categoryDescription.Locale };
            if (_categoriesState.State.Descriptions.TryGetValue(key, out var category))
            {
                category.Description = categoryDescription.Description;
            }
            else
            {
                category = categoryDescription;
                _categoriesState.State.Descriptions.Add(key, category);
            }
            await _categoriesState.WriteStateAsync();

            if (categoryDescription.TwitchCategoryId == _channelInfo.GameId && categoryDescription.Locale == _channelInfo.BroadcasterLanguage)
            {
                await OnChannelUpdate(_channelInfo);
            }
        }

        public Task<CustomCategoryDescription> GetCustomizedCategoryDescription(string twitchCategory, string locale)
        {
            var key = new CategoryKey { TwitchCategoryId = twitchCategory, Locale = locale };
            if (_categoriesState.State.Descriptions.TryGetValue(key, out var categoryDescription))
            {
                return Task.FromResult(categoryDescription);
            }
            return Task.FromResult<CustomCategoryDescription>(null);
        }

        public Task<CustomCategoryDescription[]> GetCustomizedCategoryDescriptions()
        {
            return Task.FromResult(_categoriesState.State.Descriptions.Values.ToArray());
        }
    }
}
