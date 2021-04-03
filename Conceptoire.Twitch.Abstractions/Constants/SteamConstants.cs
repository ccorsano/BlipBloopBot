using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Constants
{
    public static class SteamConstants
    {
        public static class LanguageCodes
        {
            public static readonly String ARABIC = "ar";
            public static readonly String BULGARIAN = "bg";
            public static readonly String CHINESE_SIMPLIFIED = "zh-CN";
            public static readonly String CHINESE_TRADITIONAL = "zh-TW";
            public static readonly String CZECH = "cs";
            public static readonly String DANISH = "da";
            public static readonly String DUTCH = "nl";
            public static readonly String ENGLISH = "en";
            public static readonly String FINNISH = "fi";
            public static readonly String FRENCH = "fr";
            public static readonly String GERMAN = "de";
            public static readonly String GREEK = "el";
            public static readonly String HUNGARIAN = "hu";
            public static readonly String ITALIAN = "it";
            public static readonly String JAPANESE = "ja";
            public static readonly String KOREAN = "ko";
            public static readonly String NORWEGIAN = "no";
            public static readonly String POLISH = "pl";
            public static readonly String PORTUGUESE = "pt";
            public static readonly String PORTUGUESE_BRAZIL = "pt-BR";
            public static readonly String ROMANIAN = "ro";
            public static readonly String RUSSIAN = "ru";
            public static readonly String SPANISH_SPAIN = "es";
            public static readonly String SPANISH_LATINAMERICA = "es-419";
            public static readonly String SWEDISH = "sv";
            public static readonly String THAI = "th";
            public static readonly String TURKISH = "tr";
            public static readonly String UKRAINIAN = "uk";
            public static readonly String VIETNAMESE = "vn";
        }

        public static readonly HashSet<string> SupportedLanguages = new HashSet<string>
        {
            LanguageCodes.ARABIC,
            LanguageCodes.BULGARIAN,
            LanguageCodes.CHINESE_SIMPLIFIED,
            LanguageCodes.CHINESE_TRADITIONAL,
            LanguageCodes.CZECH,
            LanguageCodes.DANISH,
            LanguageCodes.DUTCH,
            LanguageCodes.ENGLISH,
            LanguageCodes.FINNISH,
            LanguageCodes.FRENCH,
            LanguageCodes.GERMAN,
            LanguageCodes.GREEK,
            LanguageCodes.HUNGARIAN,
            LanguageCodes.ITALIAN,
            LanguageCodes.JAPANESE,
            LanguageCodes.KOREAN,
            LanguageCodes.NORWEGIAN,
            LanguageCodes.POLISH,
            LanguageCodes.PORTUGUESE,
            LanguageCodes.PORTUGUESE_BRAZIL,
            LanguageCodes.ROMANIAN,
            LanguageCodes.RUSSIAN,
            LanguageCodes.SPANISH_SPAIN,
            LanguageCodes.SPANISH_LATINAMERICA,
            LanguageCodes.SWEDISH,
            LanguageCodes.THAI,
            LanguageCodes.TURKISH,
            LanguageCodes.UKRAINIAN,
            LanguageCodes.VIETNAMESE,
        };

        public static readonly Dictionary<string, string> TwitchLanguageMapping = new Dictionary<string, string>
        {
            { TwitchConstants.LanguageCodes.ENGLISH, SteamConstants.LanguageCodes.ENGLISH },
            { TwitchConstants.LanguageCodes.FRENCH, SteamConstants.LanguageCodes.FRENCH},
            { TwitchConstants.LanguageCodes.ITALIAN, SteamConstants.LanguageCodes.ITALIAN},
            { TwitchConstants.LanguageCodes.GERMAN, SteamConstants.LanguageCodes.GERMAN},
            { TwitchConstants.LanguageCodes.SPANISH, SteamConstants.LanguageCodes.SPANISH_SPAIN},
            { TwitchConstants.LanguageCodes.DANISH, SteamConstants.LanguageCodes.DANISH},
            { TwitchConstants.LanguageCodes.HUNGARIAN, SteamConstants.LanguageCodes.HUNGARIAN},
            { TwitchConstants.LanguageCodes.DUTCH, SteamConstants.LanguageCodes.DUTCH},
            { TwitchConstants.LanguageCodes.NORWEGIAN, SteamConstants.LanguageCodes.NORWEGIAN},
            { TwitchConstants.LanguageCodes.POLISH, SteamConstants.LanguageCodes.POLISH},
            { TwitchConstants.LanguageCodes.PORTUGUESE, SteamConstants.LanguageCodes.PORTUGUESE},
            { TwitchConstants.LanguageCodes.ROMANIAN, SteamConstants.LanguageCodes.ROMANIAN},
            { TwitchConstants.LanguageCodes.FINNISH, SteamConstants.LanguageCodes.FINNISH},
            { TwitchConstants.LanguageCodes.SWEDISH, SteamConstants.LanguageCodes.SWEDISH},
            { TwitchConstants.LanguageCodes.VIETNAMESE, SteamConstants.LanguageCodes.VIETNAMESE},
            { TwitchConstants.LanguageCodes.TURKISH, SteamConstants.LanguageCodes.TURKISH},
            { TwitchConstants.LanguageCodes.CZECH, SteamConstants.LanguageCodes.CZECH},
            { TwitchConstants.LanguageCodes.GREEK, SteamConstants.LanguageCodes.GREEK},
            { TwitchConstants.LanguageCodes.BULGARIAN, SteamConstants.LanguageCodes.BULGARIAN},
            { TwitchConstants.LanguageCodes.RUSSIAN, SteamConstants.LanguageCodes.RUSSIAN},
            { TwitchConstants.LanguageCodes.UKRAINIAN, SteamConstants.LanguageCodes.UKRAINIAN},
            { TwitchConstants.LanguageCodes.ARABIC, SteamConstants.LanguageCodes.ARABIC},
            { TwitchConstants.LanguageCodes.THAI, SteamConstants.LanguageCodes.THAI},
            { TwitchConstants.LanguageCodes.CHINESE, SteamConstants.LanguageCodes.CHINESE_SIMPLIFIED},
            { TwitchConstants.LanguageCodes.JAPANESE, SteamConstants.LanguageCodes.JAPANESE},
            { TwitchConstants.LanguageCodes.CHINESE_HONGKONG, SteamConstants.LanguageCodes.CHINESE_TRADITIONAL}, // Technically, Twitch uses cn-hk (HongKong), while Steam uses cn-tw (Taiwan), so ideally they should provide more forms
            { TwitchConstants.LanguageCodes.KOREAN, SteamConstants.LanguageCodes.KOREAN},
        };
    }
}
