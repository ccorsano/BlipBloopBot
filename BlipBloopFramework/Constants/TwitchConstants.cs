using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Constants
{
    public static class TwitchConstants
    {
        public static string ENGLISH = "en";
        public static string INDONESIAN = "id";
        public static string CATALAN = "ca";
        public static string DANISH = "da";
        public static string GERMAN = "de";
        public static string SPANISH = "es";
        public static string FRENCH = "fr";
        public static string ITALIAN = "it";
        public static string HUNGARIAN = "hu";
        public static string DUTCH = "nl";
        public static string NORWEGIAN = "no";
        public static string POLISH = "pl";
        public static string PORTUGUESE = "pt";
        public static string ROMANIAN = "ro";
        public static string SLOVAK = "sk";
        public static string FINNISH = "fi";
        public static string SWEDISH = "sv";
        public static string TAGALOG = "tl";
        public static string VIETNAMESE = "vi";
        public static string TURKISH = "tr";
        public static string CZECH = "cs";
        public static string GREEK = "el";
        public static string BULGARIAN = "bg";
        public static string RUSSIAN = "ru";
        public static string UKRAINIAN = "uk";
        public static string ARABIC = "ar";
        public static string MALAY = "ms";
        public static string HINDI = "hi";
        public static string THAI = "th";
        public static string CHINESE = "zh";
        public static string JAPANESE = "ja";
        public static string CHINESE_HONGKONG = "zh-hk";
        public static string KOREAN = "ko";

        public static HashSet<string> SupportedLanguages = new HashSet<string>
        {
            ENGLISH,
            INDONESIAN,
            CATALAN,
            DANISH,
            GERMAN,
            SPANISH,
            FRENCH,
            ITALIAN,
            HUNGARIAN,
            DUTCH,
            NORWEGIAN,
            POLISH,
            PORTUGUESE,
            ROMANIAN,
            SLOVAK,
            FINNISH,
            SWEDISH,
            TAGALOG,
            VIETNAMESE,
            TURKISH,
            CZECH,
            GREEK,
            BULGARIAN,
            RUSSIAN,
            UKRAINIAN,
            ARABIC,
            MALAY,
            HINDI,
            THAI,
            CHINESE,
            JAPANESE,
            CHINESE_HONGKONG,
            KOREAN,
        };

        public const string SCOPE_ANALYTICS_READ_EXTENSIONS = "analytics:read:extensions";
        public const string SCOPE_ANALYTICS_READ_GAMES      = "analytics:read:games";
        public const string SCOPE_BITS_READ                 = "bits:read";
        public const string SCOPE_CHANNEL_EDIT_COMMERCIAL   = "channel:edit:commercial";
        public const string SCOPE_CHANNEL_MANAGE_BROADCAST  = "channel:manage:broadcast";
        public const string SCOPE_CHANNEL_MANAGE_EXTENSIONS = "channel:manage:extensions";
        public const string SCOPE_CHANNEL_MANAGE_REDEMPTIONS= "channel:manage:redemptions";
        public const string SCOPE_CHANNEL_MANAGE_VIDEOS     = "channel:manage:videos";
        public const string SCOPE_CHANNEL_READ_EDITORS      = "channel:read:editors";
        public const string SCOPE_CHANNEL_READ_HYPETRAIN    = "channel:read:hype_train";
        public const string SCOPE_CHANNEL_READ_REDEMPTIONS  = "channel:read:redemptions";
        public const string SCOPE_CHANNEL_READ_STREAMKEY    = "channel:read:stream_key";
        public const string SCOPE_CHANNEL_READ_SUBSCRIPTIONS= "channel:read:subscriptions";
        public const string SCOPE_CLIPS_EDIT                = "clips:edit";
        public const string SCOPE_MODERATION_READ           = "moderation:read";
        public const string SCOPE_USER_EDIT                 = "user:edit";
        public const string SCOPE_USER_EDIT_FOLLOWS         = "user:edit:follows";
        public const string SCOPE_USER_READ_BLOCKEDUSERS    = "user:read:blocked_users";
        public const string SCOPE_USER_MANAGE_BLOCKEDUSERS  = "user:manage:blocked_users";
        public const string SCOPE_USER_READ_BROADCAST       = "user:read:broadcast";
        public const string SCOPE_USER_READ_EMAIL           = "user:read:email";
        public const string SCOPE_USER_READ_SUBSCRIPTIONS   = "user:read:subscriptions";

        public const string SCOPE_CHANNEL_MODERATE          = "channel:moderate";
        public const string SCOPE_CHAT_EDIT                 = "chat:edit";
        public const string SCOPE_CHAT_READ                 = "chat:read";
        public const string SCOPE_WHISPERS_READ             = "whispers:read";
        public const string SCOPE_WHISPERS_EDIT             = "whispers:edit";

        public static readonly Dictionary<TwitchOAuthScopes, string> ScopesValues = new Dictionary<TwitchOAuthScopes, string>
        {
            { TwitchOAuthScopes.AnalyticsReadExtensions, SCOPE_ANALYTICS_READ_EXTENSIONS },
            { TwitchOAuthScopes.AnalyticsReadGames, SCOPE_ANALYTICS_READ_GAMES },
            { TwitchOAuthScopes.BitsRead, SCOPE_BITS_READ },
            { TwitchOAuthScopes.ChannelEditCommercial, SCOPE_CHANNEL_EDIT_COMMERCIAL },
            { TwitchOAuthScopes.ChannelManageBroadcast, SCOPE_CHANNEL_MANAGE_BROADCAST },
            { TwitchOAuthScopes.ChannelManageExtensions, SCOPE_CHANNEL_MANAGE_EXTENSIONS },
            { TwitchOAuthScopes.ChannelManageRedemptions, SCOPE_CHANNEL_MANAGE_REDEMPTIONS },
            { TwitchOAuthScopes.ChannelManageVideos, SCOPE_CHANNEL_MANAGE_VIDEOS },
            { TwitchOAuthScopes.ChannelReadEditors, SCOPE_CHANNEL_READ_EDITORS },
            { TwitchOAuthScopes.ChannelReadHypeTrain, SCOPE_CHANNEL_READ_HYPETRAIN },
            { TwitchOAuthScopes.ChannelReadRedemptions, SCOPE_CHANNEL_READ_REDEMPTIONS },
            { TwitchOAuthScopes.ChannelReadStreamKey, SCOPE_CHANNEL_READ_STREAMKEY },
            { TwitchOAuthScopes.ChannelReadSubscriptions, SCOPE_CHANNEL_READ_SUBSCRIPTIONS },
            { TwitchOAuthScopes.ClipsEdit, SCOPE_CLIPS_EDIT },
            { TwitchOAuthScopes.ModerationRead, SCOPE_MODERATION_READ },
            { TwitchOAuthScopes.UserEdit, SCOPE_USER_EDIT },
            { TwitchOAuthScopes.UserEditFollows, SCOPE_USER_EDIT_FOLLOWS },
            { TwitchOAuthScopes.UserReadBlockedUsers, SCOPE_USER_READ_BLOCKEDUSERS },
            { TwitchOAuthScopes.UserManageBlockedUsers, SCOPE_USER_MANAGE_BLOCKEDUSERS },
            { TwitchOAuthScopes.UserReadBroadcast, SCOPE_USER_READ_BROADCAST },
            { TwitchOAuthScopes.UserReadEmail, SCOPE_USER_READ_EMAIL },
            { TwitchOAuthScopes.UserReadSubscriptions, SCOPE_USER_READ_SUBSCRIPTIONS },
            { TwitchOAuthScopes.ChannelModerate, SCOPE_CHANNEL_MODERATE },
            { TwitchOAuthScopes.ChatEdit, SCOPE_CHAT_EDIT },
            { TwitchOAuthScopes.ChatRead, SCOPE_CHAT_READ },
            { TwitchOAuthScopes.WhispersRead, SCOPE_WHISPERS_READ },
            { TwitchOAuthScopes.WhispersEdit, SCOPE_WHISPERS_EDIT },
        };

        public enum TwitchOAuthScopes
        {
            AnalyticsReadExtensions,
            AnalyticsReadGames,
            BitsRead,
            ChannelEditCommercial,
            ChannelManageBroadcast,
            ChannelManageExtensions,
            ChannelManageRedemptions,
            ChannelManageVideos,
            ChannelReadEditors,
            ChannelReadHypeTrain,
            ChannelReadRedemptions,
            ChannelReadStreamKey,
            ChannelReadSubscriptions,
            ClipsEdit,
            ModerationRead,
            UserEdit,
            UserEditFollows,
            UserReadBlockedUsers,
            UserManageBlockedUsers,
            UserReadBroadcast,
            UserReadEmail,
            UserReadSubscriptions,
            ChannelModerate,
            ChatEdit,
            ChatRead,
            WhispersRead,
            WhispersEdit,
        }
    }
}
