namespace BlipBloopBot.Twitch.EventSub
{
    public class EventSubHeaders
    {
        public string MessageId { get; internal set; }
        public int    MessageRetry { get; internal set; }
        public string MessageType { get; internal set; }
        public string MessageSignature { get; internal set; }
        public string MessageTimeStamp { get; internal set; }
        public string SubscriptionType { get; internal set; }
        public string SubscriptionVersion { get; internal set; }
    }
}
