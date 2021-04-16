using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public abstract class BotCommandBase : IMessageProcessor
    {
        class ProcessorSettingsBase : IProcessorSettings
        {
            public Task ReadAsync()
            {
                return Task.CompletedTask;
            }

            public Task WriteAsync()
            {
                return Task.CompletedTask;
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

        public virtual Task OnChangeSettings(IProcessorSettings settings)
        {
            return Task.CompletedTask;
        }

        public abstract bool CanHandleMessage(in ParsedIRCMessage message);

        public abstract void OnMessage(in ParsedIRCMessage message, Action<OutgoingMessage> sendResponse);

        public abstract Task OnUpdateContext(IProcessorContext context);
    }
}
