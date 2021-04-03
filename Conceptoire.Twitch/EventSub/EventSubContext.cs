using Conceptoire.Twitch.Model.EventSub;
using Microsoft.Extensions.Logging;

namespace Conceptoire.Twitch.EventSub
{
    public class EventSubContext
    {
        public EventSubHeaders Headers { get; internal set; } = new EventSubHeaders();
        public bool IsValid { get; internal set; } = false;
        public TwitchEventSubSubscription Subscription { get; internal set; }
        public ILogger Logger { get; internal set; }
    }
}
