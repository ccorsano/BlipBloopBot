using Conceptoire.Twitch.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public abstract class BotCommandBase : IMessageProcessor
    {
        class ProcessorSettingsBase : IProcessorSettings
        {
            protected string[] Aliases { get; set; }

            private Dictionary<string, string> _parameters = new Dictionary<string, string>();

            public void LoadFromOptions(CommandOptions options)
            {
                Aliases = options.Aliases?.ToArray() ?? new string[0];
                _parameters = options.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            public void SaveToOptions(CommandOptions options)
            {
                options.Aliases = Aliases.ToArray();
                foreach ((var key, var param) in _parameters)
                {
                    options.Parameters[key] = param;
                }
            }
        }

        public Guid Id { get; set; }

        public BotCommandBase()
        {

        }

        public virtual async Task<IProcessorSettings> CreateSettings(Guid processorId, IProcessorSettings settings)
        {
            Id = processorId;
            if (settings == null)
            {
                settings = new ProcessorSettingsBase();
            }
            await OnChangeSettings(settings);
            return settings;
        }

        public virtual async Task<IProcessorSettings> LoadSettings(Guid processorId, CommandOptions options)
        {
            Id = processorId;
            var settings = new ProcessorSettingsBase();
            settings.LoadFromOptions(options);
            await OnChangeSettings(settings);
            return settings;
        }

        public virtual Task OnChangeSettings(IProcessorSettings settings)
        {
            return Task.CompletedTask;
        }

        public abstract bool CanHandleMessage(in ParsedIRCMessage message);

        public abstract void OnMessage(in ParsedIRCMessage message, Action<OutgoingMessage> sendResponse);

        public abstract Task OnUpdateContext(IProcessorContext context);

        protected bool HasMatchingCommand(in ParsedIRCMessage message, string[] aliases)
        {
            foreach (var botCommand in message.Trailing.ParseBotCommands('!'))
            {
                foreach (var alias in aliases)
                {
                    if (alias == botCommand)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
