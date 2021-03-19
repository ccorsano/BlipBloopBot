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

        public static readonly string EVENTSUB_HEADERNAME_MSGID = "twitch-eventsub-message-id";
        public static readonly string EVENTSUB_HEADERNAME_RETRY = "twitch-eventsub-message-retry";
        public static readonly string EVENTSUB_HEADERNAME_MSGTYPE = "twitch-eventsub-message-type";
        public static readonly string EVENTSUB_HEADERNAME_SIGNATURE = "twitch-eventsub-message-signature";
        public static readonly string EVENTSUB_HEADERNAME_TIMESTAMP = "twitch-eventsub-message-timestamp";
        public static readonly string EVENTSUB_HEADERNAME_SUBTYPE = "twitch-eventsub-subscription-type";
        public static readonly string EVENTSUB_HEADERNAME_SUBVERSION = "twitch-eventsub-subscription-version";

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
    }
}
