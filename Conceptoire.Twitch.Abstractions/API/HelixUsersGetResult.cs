﻿using System;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixUsersGetResult
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("broadcaster_type")]
        public string BroadcasterType { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("profile_image_url")]
        public Uri ProfileImageUrl { get; set; }

        [JsonPropertyName("offline_image_url")]
        public Uri OfflineImageUrl { get; set; }

        [JsonPropertyName("view_count")]
        public long ViewCount { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }
}