using BlipBloopBot.Model.EventSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.EventSub
{
    public interface IHandlerRegistration
    {
        public bool CanHandleEvent(EventSubContext context, TwitchEventSubEvent twitchEventSubEvent);
        public Task OnEventSubNotification(EventSubContext context, TwitchEventSubEvent twitchEventSubEvent);
    }

    public class HandlerRegistration<TEventType> : IHandlerRegistration where TEventType : TwitchEventSubEvent
    {
        private readonly Func<EventSubContext, TEventType, Task> _handler;

        public HandlerRegistration(Func<EventSubContext, TEventType, Task> handler)
        {
            _handler = handler;
        }

        public virtual bool CanHandleEvent(EventSubContext context, TwitchEventSubEvent twitchEventSubEvent)
        {
            return twitchEventSubEvent is TEventType;
        }

        public Task OnEventSubNotification(EventSubContext context, TwitchEventSubEvent twitchEventSubEvent)
        {
            if (_handler != null)
            {
                return _handler(context, twitchEventSubEvent as TEventType);
            }
            return Task.CompletedTask;
        }
    }
}
