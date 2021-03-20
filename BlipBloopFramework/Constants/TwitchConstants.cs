using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Constants
{
    public static class TwitchConstants
    {
        public static readonly string WEBHOOK_CALLBACK_VERIFICATION_PENDING = "webhook_callback_verification_pending";

        public static class EventSubHeaderNames
        {
            public const string MessageId = "twitch-eventsub-message-id";
            public const string MessageRetry = "twitch-eventsub-message-retry";
            public const string MessageType = "twitch-eventsub-message-type";
            public const string MessageSignature = "twitch-eventsub-message-signature";
            public const string MessageTimeStamp = "twitch-eventsub-message-timestamp";
            public const string SubscriptionType = "twitch-eventsub-subscription-type";
            public const string SubscriptionVersion = "twitch-eventsub-subscription-version";
        }

        public enum EventSubStatus
        {
            Enabled,
            WebhookCallVerificationPending,
            WebhookCallVerificationFailed,
            NotificationFailuresExceeded,
            AuthorizationRevoked,
            UserRemoved,
        }

        public static string GetEventSubStatusString(EventSubStatus status) =>
            status switch
            {
                EventSubStatus.Enabled => "enabled",
                EventSubStatus.WebhookCallVerificationPending => "webhook_callback_verification_pending",
                EventSubStatus.WebhookCallVerificationFailed => "webhook_callback_verification_failed",
                EventSubStatus.NotificationFailuresExceeded => "notification_failures_exceeded",
                EventSubStatus.AuthorizationRevoked => "authorization_revoked",
                EventSubStatus.UserRemoved => "user_removed",
                _ => throw new NotImplementedException()
            };

    public static class EventSubMessageTypes
        {
            public const string Notification = "notification";
            public const string WebHookCallbackVerification = "webhook_callback_verification";
        }

        public static class EventSubTypes
        {
            public const string ChannelBan = "channel.ban";
            public const string ChannelCheer = "channel.cheer";
            public const string ChannelCustomRewardAdd = "channel.channel_points_custom_reward.add";
            public const string ChannelCustomRewardUpdate = "channel.channel_points_custom_reward.update";
            public const string ChannelCustomRewardRemove = "channel.channel_points_custom_reward.remove";
            public const string ChannelCustomRewardRedemptionAdd = "channel.channel_points_custom_reward_redemption.add";
            public const string ChannelCustomRewardRedemptionUpdate = "channel.channel_points_custom_reward_redemption.update";
            public const string ChannelCustomRewardRedemptionRemove = "channel.channel_points_custom_reward_redemption.remove";
            public const string ChannelFollow = "channel.follow";
            public const string ChannelModAdd = "channel.moderator.add";
            public const string ChannelModRemove = "channel.moderator.remove";
            public const string ChannelRaid = "channel.raid";
            public const string ChannelSubscribe = "channel.subscribe";
            public const string ChannelUnban = "channel.unban";
            public const string ChannelUpdate = "channel.update";
            public const string HypeTrainBegin = "channel.hype_train.begin";
            public const string HypeTrainEnd = "channel.hype_train.end";
            public const string HypeTrainProgress = "channel.hype_train.progress";
            public const string StreamOffline = "stream.offline";
            public const string StreamOnline = "stream.online";
            public const string UserRevoke = "user.authorization.revoke";
            public const string UserUpdate = "user.update";
        }

        public static class LanguageCodes
        {
            public const string ENGLISH = "en";
            public const string INDONESIAN = "id";
            public const string CATALAN = "ca";
            public const string DANISH = "da";
            public const string GERMAN = "de";
            public const string SPANISH = "es";
            public const string FRENCH = "fr";
            public const string ITALIAN = "it";
            public const string HUNGARIAN = "hu";
            public const string DUTCH = "nl";
            public const string NORWEGIAN = "no";
            public const string POLISH = "pl";
            public const string PORTUGUESE = "pt";
            public const string ROMANIAN = "ro";
            public const string SLOVAK = "sk";
            public const string FINNISH = "fi";
            public const string SWEDISH = "sv";
            public const string TAGALOG = "tl";
            public const string VIETNAMESE = "vi";
            public const string TURKISH = "tr";
            public const string CZECH = "cs";
            public const string GREEK = "el";
            public const string BULGARIAN = "bg";
            public const string RUSSIAN = "ru";
            public const string UKRAINIAN = "uk";
            public const string ARABIC = "ar";
            public const string MALAY = "ms";
            public const string HINDI = "hi";
            public const string THAI = "th";
            public const string CHINESE = "zh";
            public const string JAPANESE = "ja";
            public const string CHINESE_HONGKONG = "zh-hk";
            public const string KOREAN = "ko";
        }

        public static HashSet<string> SupportedLanguages = new HashSet<string>
        {
            LanguageCodes.ENGLISH,
            LanguageCodes.INDONESIAN,
            LanguageCodes.CATALAN,
            LanguageCodes.DANISH,
            LanguageCodes.GERMAN,
            LanguageCodes.SPANISH,
            LanguageCodes.FRENCH,
            LanguageCodes.ITALIAN,
            LanguageCodes.HUNGARIAN,
            LanguageCodes.DUTCH,
            LanguageCodes.NORWEGIAN,
            LanguageCodes.POLISH,
            LanguageCodes.PORTUGUESE,
            LanguageCodes.ROMANIAN,
            LanguageCodes.SLOVAK,
            LanguageCodes.FINNISH,
            LanguageCodes.SWEDISH,
            LanguageCodes.TAGALOG,
            LanguageCodes.VIETNAMESE,
            LanguageCodes.TURKISH,
            LanguageCodes.CZECH,
            LanguageCodes.GREEK,
            LanguageCodes.BULGARIAN,
            LanguageCodes.RUSSIAN,
            LanguageCodes.UKRAINIAN,
            LanguageCodes.ARABIC,
            LanguageCodes.MALAY,
            LanguageCodes.HINDI,
            LanguageCodes.THAI,
            LanguageCodes.CHINESE,
            LanguageCodes.JAPANESE,
            LanguageCodes.CHINESE_HONGKONG,
            LanguageCodes.KOREAN,
        };
    }
}
