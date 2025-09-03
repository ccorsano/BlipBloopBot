using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.Model.EventSub
{
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubChannelBanEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubChannelCheerEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubChannelPointsCustomRewardDefinitionEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubChannelPointsCustomRewardRedemptionEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubChannelFollowEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubChannelModAddEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubChannelModRemoveEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubChannelRaidEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubChannelSubscribeEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubChannelUnbanEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubChannelUpdateEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubHypeTrainBeginEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubHypeTrainEndEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubHypeTrainProgressEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubStreamOfflineEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubStreamOnlineEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubUserRevokeEvent>))]
    [JsonSerializable(typeof(TwitchEventSubCallbackPayload<TwitchEventSubUserUpdateEvent>))]
    internal sealed partial class TwitchEventSubJsonContext : JsonSerializerContext
    {
    }
}
