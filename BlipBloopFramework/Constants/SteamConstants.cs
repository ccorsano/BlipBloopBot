using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Constants
{
    public static class SteamConstants
    {
        public static String ARABIC = "ar";
        public static String BULGARIAN = "bg";
        public static String CHINESE_SIMPLIFIED = "zh-CN";
        public static String CHINESE_TRADITIONAL = "zh-TW";
        public static String CZECH = "cs";
        public static String DANISH = "da";
        public static String DUTCH = "nl";
        public static String ENGLISH = "en";
        public static String FINNISH = "fi";
        public static String FRENCH = "fr";
        public static String GERMAN = "de";
        public static String GREEK = "el";
        public static String HUNGARIAN = "hu";
        public static String ITALIAN = "it";
        public static String JAPANESE = "ja";
        public static String KOREAN = "ko";
        public static String NORWEGIAN = "no";
        public static String POLISH = "pl";
        public static String PORTUGUESE = "pt";
        public static String PORTUGUESE_BRAZIL = "pt-BR";
        public static String ROMANIAN = "ro";
        public static String RUSSIAN = "ru";
        public static String SPANISH_SPAIN = "es";
        public static String SPANISH_LATINAMERICA = "es-419";
        public static String SWEDISH = "sv";
        public static String THAI = "th";
        public static String TURKISH = "tr";
        public static String UKRAINIAN = "uk";
        public static String VIETNAMESE = "vn";

        public static readonly HashSet<string> SupportedLanguages = new HashSet<string>
        {
            ARABIC,
            BULGARIAN,
            CHINESE_SIMPLIFIED,
            CHINESE_TRADITIONAL,
            CZECH,
            DANISH,
            DUTCH,
            ENGLISH,
            FINNISH,
            FRENCH,
            GERMAN,
            GREEK,
            HUNGARIAN,
            ITALIAN,
            JAPANESE,
            KOREAN,
            NORWEGIAN,
            POLISH,
            PORTUGUESE,
            PORTUGUESE_BRAZIL,
            ROMANIAN,
            RUSSIAN,
            SPANISH_SPAIN,
            SPANISH_LATINAMERICA,
            SWEDISH,
            THAI,
            TURKISH,
            UKRAINIAN,
            VIETNAMESE,
        };

        public static readonly Dictionary<string, string> TwitchLanguageMapping = new Dictionary<string, string>
        {
            { TwitchConstants.ENGLISH, SteamConstants.ENGLISH },
            { TwitchConstants.FRENCH, SteamConstants.FRENCH},
            { TwitchConstants.ITALIAN, SteamConstants.ITALIAN},
            { TwitchConstants.GERMAN, SteamConstants.GERMAN},
            { TwitchConstants.SPANISH, SteamConstants.SPANISH_SPAIN},
            { TwitchConstants.DANISH, SteamConstants.DANISH},
            { TwitchConstants.HUNGARIAN, SteamConstants.HUNGARIAN},
            { TwitchConstants.DUTCH, SteamConstants.DUTCH},
            { TwitchConstants.NORWEGIAN, SteamConstants.NORWEGIAN},
            { TwitchConstants.POLISH, SteamConstants.POLISH},
            { TwitchConstants.PORTUGUESE, SteamConstants.PORTUGUESE},
            { TwitchConstants.ROMANIAN, SteamConstants.ROMANIAN},
            { TwitchConstants.FINNISH, SteamConstants.FINNISH},
            { TwitchConstants.SWEDISH, SteamConstants.SWEDISH},
            { TwitchConstants.VIETNAMESE, SteamConstants.VIETNAMESE},
            { TwitchConstants.TURKISH, SteamConstants.TURKISH},
            { TwitchConstants.CZECH, SteamConstants.CZECH},
            { TwitchConstants.GREEK, SteamConstants.GREEK},
            { TwitchConstants.BULGARIAN, SteamConstants.BULGARIAN},
            { TwitchConstants.RUSSIAN, SteamConstants.RUSSIAN},
            { TwitchConstants.UKRAINIAN, SteamConstants.UKRAINIAN},
            { TwitchConstants.ARABIC, SteamConstants.ARABIC},
            { TwitchConstants.THAI, SteamConstants.THAI},
            { TwitchConstants.CHINESE, SteamConstants.CHINESE_SIMPLIFIED},
            { TwitchConstants.JAPANESE, SteamConstants.JAPANESE},
            { TwitchConstants.CHINESE_HONGKONG, SteamConstants.CHINESE_TRADITIONAL}, // Technically, Twitch uses cn-hk (HongKong), while Steam uses cn-tw (Taiwan), so ideally they should provide more forms
            { TwitchConstants.KOREAN, SteamConstants.KOREAN},
        };
    }
}
