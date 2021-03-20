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
            public static readonly string MessageId = "twitch-eventsub-message-id";
            public static readonly string MessageRetry = "twitch-eventsub-message-retry";
            public static readonly string MessageType = "twitch-eventsub-message-type";
            public static readonly string MessageSignature = "twitch-eventsub-message-signature";
            public static readonly string MessageTimeStamp = "twitch-eventsub-message-timestamp";
            public static readonly string SubscriptionType = "twitch-eventsub-subscription-type";
            public static readonly string SubscriptionVersion = "twitch-eventsub-subscription-version";
        }

        public static class EventSubTypes
        {
            public static readonly string ChannelBan = "channel.ban";
            public static readonly string ChannelCheer = "channel.cheer";
            public static readonly string ChannelCustomRewardAdd = "channel.channel_points_custom_reward.add";
            public static readonly string ChannelCustomRewardUpdate = "channel.channel_points_custom_reward.update";
            public static readonly string ChannelCustomRewardRemove = "channel.channel_points_custom_reward.remove";
            public static readonly string ChannelCustomRewardRedemptionAdd = "channel.channel_points_custom_reward_redemption.add";
            public static readonly string ChannelCustomRewardRedemptionUpdate = "channel.channel_points_custom_reward_redemption.update";
            public static readonly string ChannelCustomRewardRedemptionRemove = "channel.channel_points_custom_reward_redemption.remove";
            public static readonly string ChannelFollow = "channel.follow";
            public static readonly string ChannelModAdd = "channel.moderator.add";
            public static readonly string ChannelModRemove = "channel.moderator.remove";
            public static readonly string ChannelRaid = "channel.raid";
            public static readonly string ChannelSubscribe = "channel.subscribe";
            public static readonly string ChannelUnban = "channel.unban";
            public static readonly string ChannelUpdate = "channel.update";
            public static readonly string HypeTrainBegin = "channel.hype_train.begin";
            public static readonly string HypeTrainEnd = "channel.hype_train.end";
            public static readonly string HypeTrainProgress = "channel.hype_train.progress";
            public static readonly string StreamOffline = "channel.hype_train.end";
            public static readonly string StreamOnline = "channel.hype_train.progress";
            public static readonly string UserRevoke = "user.authorization.revoke";
            public static readonly string UserUpdate = "user.update";
        }

        public static class LanguageCodes
        {
            public static readonly string ENGLISH = "en";
            public static readonly string INDONESIAN = "id";
            public static readonly string CATALAN = "ca";
            public static readonly string DANISH = "da";
            public static readonly string GERMAN = "de";
            public static readonly string SPANISH = "es";
            public static readonly string FRENCH = "fr";
            public static readonly string ITALIAN = "it";
            public static readonly string HUNGARIAN = "hu";
            public static readonly string DUTCH = "nl";
            public static readonly string NORWEGIAN = "no";
            public static readonly string POLISH = "pl";
            public static readonly string PORTUGUESE = "pt";
            public static readonly string ROMANIAN = "ro";
            public static readonly string SLOVAK = "sk";
            public static readonly string FINNISH = "fi";
            public static readonly string SWEDISH = "sv";
            public static readonly string TAGALOG = "tl";
            public static readonly string VIETNAMESE = "vi";
            public static readonly string TURKISH = "tr";
            public static readonly string CZECH = "cs";
            public static readonly string GREEK = "el";
            public static readonly string BULGARIAN = "bg";
            public static readonly string RUSSIAN = "ru";
            public static readonly string UKRAINIAN = "uk";
            public static readonly string ARABIC = "ar";
            public static readonly string MALAY = "ms";
            public static readonly string HINDI = "hi";
            public static readonly string THAI = "th";
            public static readonly string CHINESE = "zh";
            public static readonly string JAPANESE = "ja";
            public static readonly string CHINESE_HONGKONG = "zh-hk";
            public static readonly string KOREAN = "ko";
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
