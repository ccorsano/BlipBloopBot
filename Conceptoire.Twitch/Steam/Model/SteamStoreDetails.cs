using System;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.Steam.Model
{
    public class Demo
    {
        public long Appid { get; set; }

        public string Description { get; set; }
    }

    public class Category
    {
        public long Id { get; set; }

        public string Description { get; set; }
    }

    public class Genre
    {
        public string Id { get; set; }

        public string Description { get; set; }
    }

    public class Screenshot
    {
        public long Id { get; set; }

        [JsonPropertyName("path_thumbnail")]
        public Uri PathThumbnail { get; set; }

        [JsonPropertyName("path_full")]
        public Uri PathFull { get; set; }
    }

    public class Recommendations
    {
        public long Total { get; set; }
    }

    public class Highlighted
    {
        public string Name { get; set; }

        public Uri Path { get; set; }
    }

    public class Achievements
    {
        public long Total { get; set; }

        public Highlighted[] Highlighted { get; set; }
    }

    public class ReleaseDate
    {
        [JsonPropertyName("coming_soon")]
        public bool ComingSoon { get; set; }

        public string Date { get; set; }
    }

    public class SupportInfo
    {
        public Uri Url { get; set; }

        public string Email { get; set; }
    }


    public class SteamStoreDetails
    {
        public string Type { get; set; }

        public string Name { get; set; }

        [JsonPropertyName("steam_appid")]
        public long SteamAppid { get; set; }

        //[JsonPropertyName("required_age")]
        //public string RequiredAge { get; set; }

        [JsonPropertyName("is_free")]
        public bool IsFree { get; set; }

        [JsonPropertyName("controller_support")]
        public string ControllerSupport { get; set; }

        public long[] Dlc { get; set; }

        [JsonPropertyName("detailed_description")]
        public string DetailedDescription { get; set; }

        [JsonPropertyName("about_the_game")]
        public string AboutTheGame { get; set; }

        [JsonPropertyName("short_description")]
        public string ShortDescription { get; set; }

        [JsonPropertyName("supported_languages")]
        public string SupportedLanguages { get; set; }

        [JsonPropertyName("header_image")]
        public Uri HeaderImage { get; set; }

        public Uri Website { get; set; }

        //[JsonPropertyName("pc_requirements")]
        //public Requirements PcRequirements { get; set; }

        //[JsonPropertyName("mac_requirements")]
        //public MacRequirements MacRequirements { get; set; }

        //[JsonPropertyName("linux_requirements")]
        //public Requirements LinuxRequirements { get; set; }

        [JsonPropertyName("legal_notice")]
        public string LegalNotice { get; set; }

        public string[] Developers { get; set; }

        public string[] Publishers { get; set; }

        public Demo[] Demos { get; set; }

        public long[] Packages { get; set; }

        public object[] PackageGroups { get; set; }

        //public Platforms Platforms { get; set; }

        public Category[] Categories { get; set; }

        public Genre[] Genres { get; set; }

        public Screenshot[] Screenshots { get; set; }

        //public Movie[] Movies { get; set; }

        public Recommendations Recommendations { get; set; }

        public Achievements Achievements { get; set; }

        [JsonPropertyName("release_date")]
        public ReleaseDate ReleaseDate { get; set; }

        [JsonPropertyName("support_info")]
        public SupportInfo SupportInfo { get; set; }

        public Uri Background { get; set; }

        //[JsonPropertyName("content_descriptors")]
        //public ContentDescriptors ContentDescriptors { get; set; }
    }
}