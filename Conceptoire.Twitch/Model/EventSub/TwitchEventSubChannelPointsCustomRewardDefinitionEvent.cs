using System;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.Model.EventSub
{
    public class TwitchEventSubChannelPointsCustomRewardDefinitionEvent : TwitchEventSubEvent
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("is_paused")]
        public bool IsPaused { get; set; }

        [JsonPropertyName("is_in_stock")]
        public bool IsInStock { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("cost")]
        public long Cost { get; set; }

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; }

        [JsonPropertyName("is_user_input_required")]
        public bool IsUserInputRequired { get; set; }

        [JsonPropertyName("should_redemptions_skip_request_queue")]
        public bool ShouldRedemptionsSkipRequestQueue { get; set; }

        [JsonPropertyName("cooldown_expires_at")]
        public DateTimeOffset? CooldownExpiresAt { get; set; }

        [JsonPropertyName("redemptions_redeemed_current_stream")]
        public int RedemptionsRedeemedCurrentStream { get; set; }

        [JsonPropertyName("max_per_stream")]
        public CustomRewardLimit MaxPerStream { get; set; }

        [JsonPropertyName("max_per_user_per_stream")]
        public CustomRewardLimit MaxPerUserPerStream { get; set; }

        [JsonPropertyName("global_cooldown")]
        public GlobalCooldown GlobalCooldown { get; set; }

        [JsonPropertyName("background_color")]
        public string BackgroundColor { get; set; }

        [JsonPropertyName("image")]
        public Image Image { get; set; }

        [JsonPropertyName("default_image")]
        public Image DefaultImage { get; set; }
    }

    public class CustomRewardLimit
    {
        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("value")]
        public long Value { get; set; }
    }

    public class GlobalCooldown
    {
        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("seconds")]
        public long Seconds { get; set; }
    }

    public class Image
    {
        [JsonPropertyName("url_1x")]
        public Uri Url1X { get; set; }

        [JsonPropertyName("url_2x")]
        public Uri Url2X { get; set; }

        [JsonPropertyName("url_4x")]
        public Uri Url4X { get; set; }
    }
}
