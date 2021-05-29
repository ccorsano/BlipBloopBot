using System.Collections.Generic;

namespace Conceptoire.Twitch.Constants
{
    public static class SteamConstants
    {
        public static class LanguageCodes
        {
            public static readonly string ARABIC = "ar";
            public static readonly string BULGARIAN = "bg";
            public static readonly string CHINESE_SIMPLIFIED = "zh-CN";
            public static readonly string CHINESE_TRADITIONAL = "zh-TW";
            public static readonly string CZECH = "cs";
            public static readonly string DANISH = "da";
            public static readonly string DUTCH = "nl";
            public static readonly string ENGLISH = "en";
            public static readonly string FINNISH = "fi";
            public static readonly string FRENCH = "fr";
            public static readonly string GERMAN = "de";
            public static readonly string GREEK = "el";
            public static readonly string HUNGARIAN = "hu";
            public static readonly string ITALIAN = "it";
            public static readonly string JAPANESE = "ja";
            public static readonly string KOREAN = "ko";
            public static readonly string NORWEGIAN = "no";
            public static readonly string POLISH = "pl";
            public static readonly string PORTUGUESE = "pt";
            public static readonly string PORTUGUESE_BRAZIL = "pt-BR";
            public static readonly string ROMANIAN = "ro";
            public static readonly string RUSSIAN = "ru";
            public static readonly string SPANISH_SPAIN = "es";
            public static readonly string SPANISH_LATINAMERICA = "es-419";
            public static readonly string SWEDISH = "sv";
            public static readonly string THAI = "th";
            public static readonly string TURKISH = "tr";
            public static readonly string UKRAINIAN = "uk";
            public static readonly string VIETNAMESE = "vn";
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

        public static class APILanguageCodes
        {
            public static readonly string ARABIC = "arabic";
            public static readonly string BULGARIAN = "bulgarian";
            public static readonly string CHINESE_SIMPLIFIED = "schinese";
            public static readonly string CHINESE_TRADITIONAL = "tchinese";
            public static readonly string CZECH = "czech";
            public static readonly string DANISH = "danish";
            public static readonly string DUTCH = "dutch";
            public static readonly string ENGLISH = "english";
            public static readonly string FINNISH = "finnish";
            public static readonly string FRENCH = "french";
            public static readonly string GERMAN = "german";
            public static readonly string GREEK = "greek";
            public static readonly string HUNGARIAN = "hungarian";
            public static readonly string ITALIAN = "italian";
            public static readonly string JAPANESE = "japanese";
            public static readonly string KOREAN = "koreana";
            public static readonly string NORWEGIAN = "norwegian";
            public static readonly string POLISH = "polish";
            public static readonly string PORTUGUESE = "portuguese";
            public static readonly string PORTUGUESE_BRAZIL = "brazilian";
            public static readonly string ROMANIAN = "romanian";
            public static readonly string RUSSIAN = "russian";
            public static readonly string SPANISH_SPAIN = "spanish";
            public static readonly string SPANISH_LATINAMERICA = "latam";
            public static readonly string SWEDISH = "swedish";
            public static readonly string THAI = "thai";
            public static readonly string TURKISH = "turkish";
            public static readonly string UKRAINIAN = "ukrainian";
            public static readonly string VIETNAMESE = "vietnames";
        }

        public static readonly Dictionary<string, string> SteamLanguageCodeToAPILanguageCode = new Dictionary<string, string>
        {
            { LanguageCodes.ENGLISH, APILanguageCodes.ENGLISH },
            { LanguageCodes.FRENCH, APILanguageCodes.FRENCH},
            { LanguageCodes.ITALIAN, APILanguageCodes.ITALIAN},
            { LanguageCodes.GERMAN, APILanguageCodes.GERMAN},
            { LanguageCodes.SPANISH_SPAIN, APILanguageCodes.SPANISH_SPAIN},
            { LanguageCodes.DANISH, APILanguageCodes.DANISH},
            { LanguageCodes.HUNGARIAN, APILanguageCodes.HUNGARIAN},
            { LanguageCodes.DUTCH, APILanguageCodes.DUTCH},
            { LanguageCodes.NORWEGIAN, APILanguageCodes.NORWEGIAN},
            { LanguageCodes.POLISH, APILanguageCodes.POLISH},
            { LanguageCodes.PORTUGUESE, APILanguageCodes.PORTUGUESE},
            { LanguageCodes.ROMANIAN, APILanguageCodes.ROMANIAN},
            { LanguageCodes.FINNISH, APILanguageCodes.FINNISH},
            { LanguageCodes.SWEDISH, APILanguageCodes.SWEDISH},
            { LanguageCodes.VIETNAMESE, APILanguageCodes.VIETNAMESE},
            { LanguageCodes.TURKISH, APILanguageCodes.TURKISH},
            { LanguageCodes.CZECH, APILanguageCodes.CZECH},
            { LanguageCodes.GREEK, APILanguageCodes.GREEK},
            { LanguageCodes.BULGARIAN, APILanguageCodes.BULGARIAN},
            { LanguageCodes.RUSSIAN, APILanguageCodes.RUSSIAN},
            { LanguageCodes.UKRAINIAN, APILanguageCodes.UKRAINIAN},
            { LanguageCodes.ARABIC, APILanguageCodes.ARABIC},
            { LanguageCodes.THAI, APILanguageCodes.THAI},
            { LanguageCodes.CHINESE_SIMPLIFIED, APILanguageCodes.CHINESE_SIMPLIFIED},
            { LanguageCodes.JAPANESE, APILanguageCodes.JAPANESE},
            { LanguageCodes.CHINESE_TRADITIONAL, APILanguageCodes.CHINESE_TRADITIONAL},
            { LanguageCodes.KOREAN, APILanguageCodes.KOREAN},
        };

        public static readonly Dictionary<string, string> TwitchLanguageMapping = new Dictionary<string, string>
        {
            { TwitchConstants.LanguageCodes.ENGLISH, LanguageCodes.ENGLISH },
            { TwitchConstants.LanguageCodes.FRENCH, LanguageCodes.FRENCH},
            { TwitchConstants.LanguageCodes.ITALIAN, LanguageCodes.ITALIAN},
            { TwitchConstants.LanguageCodes.GERMAN, LanguageCodes.GERMAN},
            { TwitchConstants.LanguageCodes.SPANISH, LanguageCodes.SPANISH_SPAIN},
            { TwitchConstants.LanguageCodes.DANISH, LanguageCodes.DANISH},
            { TwitchConstants.LanguageCodes.HUNGARIAN, LanguageCodes.HUNGARIAN},
            { TwitchConstants.LanguageCodes.DUTCH, LanguageCodes.DUTCH},
            { TwitchConstants.LanguageCodes.NORWEGIAN, LanguageCodes.NORWEGIAN},
            { TwitchConstants.LanguageCodes.POLISH, LanguageCodes.POLISH},
            { TwitchConstants.LanguageCodes.PORTUGUESE, LanguageCodes.PORTUGUESE},
            { TwitchConstants.LanguageCodes.ROMANIAN, LanguageCodes.ROMANIAN},
            { TwitchConstants.LanguageCodes.FINNISH, LanguageCodes.FINNISH},
            { TwitchConstants.LanguageCodes.SWEDISH, LanguageCodes.SWEDISH},
            { TwitchConstants.LanguageCodes.VIETNAMESE, LanguageCodes.VIETNAMESE},
            { TwitchConstants.LanguageCodes.TURKISH, LanguageCodes.TURKISH},
            { TwitchConstants.LanguageCodes.CZECH, LanguageCodes.CZECH},
            { TwitchConstants.LanguageCodes.GREEK, LanguageCodes.GREEK},
            { TwitchConstants.LanguageCodes.BULGARIAN, LanguageCodes.BULGARIAN},
            { TwitchConstants.LanguageCodes.RUSSIAN, LanguageCodes.RUSSIAN},
            { TwitchConstants.LanguageCodes.UKRAINIAN, LanguageCodes.UKRAINIAN},
            { TwitchConstants.LanguageCodes.ARABIC, LanguageCodes.ARABIC},
            { TwitchConstants.LanguageCodes.THAI, LanguageCodes.THAI},
            { TwitchConstants.LanguageCodes.CHINESE, LanguageCodes.CHINESE_SIMPLIFIED},
            { TwitchConstants.LanguageCodes.JAPANESE, LanguageCodes.JAPANESE},
            { TwitchConstants.LanguageCodes.CHINESE_HONGKONG, LanguageCodes.CHINESE_TRADITIONAL}, // Technically, Twitch uses cn-hk (HongKong), while Steam uses cn-tw (Taiwan), so ideally they should provide more forms
            { TwitchConstants.LanguageCodes.KOREAN, LanguageCodes.KOREAN},
        };
    }
}
