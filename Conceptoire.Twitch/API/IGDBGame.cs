using System;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class IGDBGame
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("age_ratings")]
        public int[] AgeRatings { get; set; }

        [JsonPropertyName("aggregated_rating")]
        public double AggregatedRating { get; set; }

        [JsonPropertyName("aggregated_rating_count")]
        public int AggregatedRatingCount { get; set; }

        [JsonPropertyName("alternative_names")]
        public int[] AlternativeNames { get; set; }

        [JsonPropertyName("artworks")]
        public int[] Artworks { get; set; }

        [JsonPropertyName("category")]
        public int Category { get; set; }

        [JsonPropertyName("collection")]
        public int Collection { get; set; }

        [JsonPropertyName("cover")]
        public int Cover { get; set; }

        [JsonPropertyName("created_at")]
        public int CreatedAt { get; set; }

        [JsonPropertyName("external_games")]
        public int[] ExternalGames { get; set; }

        [JsonPropertyName("first_release_date")]
        public int FirstReleaseDate { get; set; }

        [JsonPropertyName("follows")]
        public int Follows { get; set; }

        [JsonPropertyName("game_engines")]
        public int[] GameEngines { get; set; }

        [JsonPropertyName("game_modes")]
        public int[] GameModes { get; set; }

        [JsonPropertyName("genres")]
        public int[] Genres { get; set; }

        [JsonPropertyName("hypes")]
        public int Hypes { get; set; }

        [JsonPropertyName("involved_companies")]
        public int[] InvolvedCompanies { get; set; }

        [JsonPropertyName("keywords")]
        public int[] Keywords { get; set; }

        [JsonPropertyName("multiplayer_modes")]
        public int[] MultiplayerModes { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("platforms")]
        public int[] Platforms { get; set; }

        [JsonPropertyName("player_perspectives")]
        public int[] PlayerPerspectives { get; set; }

        [JsonPropertyName("rating")]
        public double Rating { get; set; }

        [JsonPropertyName("rating_count")]
        public int RatingCount { get; set; }

        [JsonPropertyName("release_dates")]
        public int[] ReleaseDates { get; set; }

        [JsonPropertyName("screenshots")]
        public int[] Screenshots { get; set; }

        [JsonPropertyName("similar_games")]
        public int[] SimilarGames { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("storyline")]
        public string Storyline { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("tags")]
        public int[] Tags { get; set; }

        [JsonPropertyName("themes")]
        public int[] Themes { get; set; }

        [JsonPropertyName("total_rating")]
        public double TotalRating { get; set; }

        [JsonPropertyName("total_rating_count")]
        public int TotalRatingCount { get; set; }

        [JsonPropertyName("updated_at")]
        public int UpdatedAt { get; set; }

        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("videos")]
        public int[] Videos { get; set; }

        [JsonPropertyName("websites")]
        public int[] Websites { get; set; }

        [JsonPropertyName("checksum")]
        public Guid Checksum { get; set; }
    }
}
