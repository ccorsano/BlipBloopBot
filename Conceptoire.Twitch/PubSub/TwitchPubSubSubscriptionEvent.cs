using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.PubSub
{
    public class TwitchPubSubSubscriptionEvent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("data")]
        public TwitchPubSubSubscriptionData Data { get; set; }
    }

    public class TwitchPubSubSubscriptionData
    {
        [JsonPropertyName("topic")]
        public string Topic { get; set; }

        [JsonPropertyName("message")]
        public TwitchPubSubSubscriptionMessage Message { get; set; }
    }

    public class TwitchPubSubSubscriptionMessage
    {
        [JsonPropertyName("user_name")]
        public string UserName { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("channel_name")]
        public string ChannelName { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; }

        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; set; }

        [JsonPropertyName("sub_plan")]
        public string SubPlan { get; set; }

        [JsonPropertyName("sub_plan_name")]
        public string SubPlanName { get; set; }

        [JsonPropertyName("cumulative_months")]
        public ushort CumulativeMonths { get; set; }

        [JsonPropertyName("streak_months")]
        public ushort StreakMonths { get; set; }

        [JsonPropertyName("context")]
        public string Context { get; set; }

        [JsonPropertyName("is_gift")]
        public bool IsGift { get; set; }

        [JsonPropertyName("recipient_id")]
        public bool RecipientId { get; set; }

        [JsonPropertyName("recipient_user_name")]
        public bool RecipientUserName { get; set; }

        [JsonPropertyName("multi_month_duration")]
        public ushort MultiMonthDuration { get; set; }

        [JsonPropertyName("sub_message")]
        public TwitchPubSubSubscriptionSubMessage SubMessage { get; set; }
    }

    public class TwitchPubSubSubscriptionSubMessage
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("emotes")]
        public TwitchPubSubSubscriptionEmote[] Emotes { get; set; }
    }

    public class TwitchPubSubSubscriptionEmote
    {
        [JsonPropertyName("start")]
        public long Start { get; set; }

        [JsonPropertyName("end")]
        public long End { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}
