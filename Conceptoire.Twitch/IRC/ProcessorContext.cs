using Conceptoire.Twitch.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public class ProcessorContext : IProcessorContext
    {
        public string ChannelId { get; set; }

        public string ChannelName { get; set; }

        public string Language { get; set; }

        public string CategoryId { get; set; }

        public string CustomCategoryDescription { get; set; }

        private Dictionary<Guid, IProcessorState> _components;

        public TState GetState<TState>(Guid processorId) where TState : class, IProcessorState
        {
            if (_components.TryGetValue(processorId, out var state))
            {
                return state as TState;
            }
            return null;
        }

        public bool SetState<TState>(Guid processorId, TState state) where TState : class, IProcessorState
        {
            return _components.TryAdd(processorId, state);
        }
    }
}
