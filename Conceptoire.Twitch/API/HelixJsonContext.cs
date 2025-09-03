using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    [JsonSerializable(typeof(HelixCategoriesSearchEntry))]
    [JsonSerializable(typeof(HelixChannelEditor))]
    [JsonSerializable(typeof(HelixChannelInfo))]
    [JsonSerializable(typeof(HelixChannelModerator))]
    [JsonSerializable(typeof(HelixChannelSearchResult))]
    [JsonSerializable(typeof(HelixClip))]
    [JsonSerializable(typeof(HelixEventSubSubscriptionCreateRequest))]
    [JsonSerializable(typeof(HelixEventSubSubscriptionData))]
    [JsonSerializable(typeof(HelixEventSubTransport))]
    [JsonSerializable(typeof(HelixExtensionLiveChannel))]
    [JsonSerializable(typeof(HelixGetStreamsEntry))]
    [JsonSerializable(typeof(HelixUsersGetResult))]
    [JsonSerializable(typeof(HelixValidateTokenResponse))]
    [JsonSerializable(typeof(HelixVideoInfo))]
    [JsonSerializable(typeof(HelixEventSubSubscriptionsListReponse))]
    [JsonSerializable(typeof(HelixPaginatedResponse<HelixEventSubSubscriptionData>))]
    [JsonSerializable(typeof(HelixPaginatedResponse<HelixGetStreamsEntry>))]
    [JsonSerializable(typeof(HelixPaginatedResponse<HelixVideoInfo>))]
    [JsonSerializable(typeof(HelixPaginatedResponse<HelixClip>))]
    [JsonSerializable(typeof(HelixPaginatedResponse<HelixCategoriesSearchEntry>))]
    [JsonSerializable(typeof(HelixPaginatedResponse<HelixChannelModerator>))]
    [JsonSerializable(typeof(HelixPaginatedResponse<HelixExtensionLiveChannel>))]
    internal partial class HelixJsonContext : JsonSerializerContext
    {
    }
}
