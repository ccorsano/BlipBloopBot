using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.PubSub
{
    public class ChannelPointsV1 : IPubSubDataObject
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("data")]
        public ChannelPointsData Data { get; set; }
    }

    public class ChannelPointsData
    {
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonPropertyName("redemption")]
        public ChannelPointsRedemption Redemption { get; set; }
    }

    public class ChannelPointsRedemption
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("user")]
        public ChannelPointsUser User { get; set; }

        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; }

        [JsonPropertyName("redeemed_at")]
        public DateTimeOffset RedeemedAt { get; set; }

        [JsonPropertyName("reward")]
        public ChannelPointsReward Reward { get; set; }

        [JsonPropertyName("user_input")]
        public string UserInput { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class ChannelPointsUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
    }

    public class ChannelPointsReward
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; }

        [JsonPropertyName("cost")]
        public long Cost { get; set; }

        [JsonPropertyName("is_user_input_required")]
        public bool IsUserInputRequired { get; set; }

        [JsonPropertyName("is_sub_only")]
        public bool IsSubOnly { get; set; }

        [JsonPropertyName("image")]
        public ChannelPointsImage Image { get; set; }

        [JsonPropertyName("default_image")]
        public ChannelPointsImage DefaultImage { get; set; }

        [JsonPropertyName("background_color")]
        public string BackgroundColor { get; set; }

        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("is_paused")]
        public bool IsPaused { get; set; }

        [JsonPropertyName("is_in_stock")]
        public bool IsInStock { get; set; }

        [JsonPropertyName("max_per_stream")]
        public ChannelPointsMaxPerStream MaxPerStream { get; set; }

        [JsonPropertyName("should_redemptions_skip_request_queue")]
        public bool ShouldRedemptionsSkipRequestQueue { get; set; }
    }

    public class ChannelPointsImage
    {
        [JsonPropertyName("url_1x")]
        public Uri Url1X { get; set; }

        [JsonPropertyName("url_2x")]
        public Uri Url2X { get; set; }

        [JsonPropertyName("url_4x")]
        public Uri Url4X { get; set; }
    }

    public class ChannelPointsMaxPerStream
    {
        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("max_per_stream")]
        public long MaxPerStream { get; set; }
    }
}
