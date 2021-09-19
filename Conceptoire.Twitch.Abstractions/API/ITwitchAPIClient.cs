using Conceptoire.Twitch.Constants;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.API
{
    public interface ITwitchAPIClient
    {
        Task CreateEventSubChannelUpdateSubscription(string broadcasterId, Uri callback, string secret, CancellationToken? cancellationToken = null);
        Task<HelixEventSubSubscriptionData> CreateEventSubSubscription(HelixEventSubSubscriptionCreateRequest request, CancellationToken cancellationToken = default);
        Task DeleteEventSubSubscription(string subscriptionId, CancellationToken cancellationToken = default);
        IAsyncEnumerable<HelixChannelModerator> EnumerateChannelModeratorsAsync(string broadcasterId, CancellationToken cancellationToken = default);
        IAsyncEnumerable<HelixEventSubSubscriptionData> EnumerateEventSubSubscriptions(TwitchConstants.EventSubStatus? status = null, CancellationToken cancellationToken = default);
        IAsyncEnumerable<HelixCategoriesSearchEntry> EnumerateTopGamesAsync(CancellationToken cancellationToken = default);
        IAsyncEnumerable<HelixCategoriesSearchEntry> EnumerateTwitchCategoriesAsync(CancellationToken cancellationToken = default);
        IAsyncEnumerable<HelixCategoriesSearchEntry> EnumerateTwitchCategoriesAsync(string query, CancellationToken cancellationToken = default);
        IAsyncEnumerable<HelixClip> EnumerateTwitchChannelClipsAsync(string channelId, DateTime? startedAt = null, DateTime? endedAt = null, CancellationToken cancellationToken = default);
        IAsyncEnumerable<HelixVideoInfo> EnumerateTwitchChannelVideosAsync(string channelId, string videoType = null, CancellationToken cancellationToken = default);
        Task<HelixChannelInfo> GetChannelInfoAsync(string broadcasterId, CancellationToken cancellationToken = default);
        Task<HelixCategoriesSearchEntry[]> GetGamesInfo(string[] categoryIds, CancellationToken cancellationToken = default);
        Task<HelixChannelEditor[]> GetHelixChannelEditorsAsync(string broadcasterId, CancellationToken cancellationToken = default);
        Task<HelixUsersGetResult[]> GetUsersAsync(IEnumerable<string> ids, IEnumerable<string> logins, CancellationToken cancellationToken = default);
        Task<HelixUsersGetResult[]> GetUsersByIdAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
        Task<HelixUsersGetResult[]> GetUsersByLoginAsync(IEnumerable<string> logins, CancellationToken cancellationToken = default);
        Task<HelixChannelSearchResult[]> SearchChannelsAsync(string channelQuery, CancellationToken cancellationToken = default);
        Task<HelixValidateTokenResponse> ValidateToken(CancellationToken cancellationToken = default);
    }
}