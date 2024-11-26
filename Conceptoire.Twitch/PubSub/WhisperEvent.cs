using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.PubSub
{
    public class WhisperEvent : IPubSubDataObject
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        // Ignore the string representation of the data object, use the DataObject
        //[JsonPropertyName("data")]
        //public string Data { get; set; }

        [JsonPropertyName("data_object")]
        public WhisperDataObject DataObject { get; set; }
    }

    public class WhisperDataObject
    {
        [JsonPropertyName("message_id")]
        public Guid MessageId { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("thread_id")]
        public string ThreadId { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("sent_ts")]
        public long SentTs { get; set; }

        [JsonPropertyName("from_id")]
        public long FromId { get; set; }

        [JsonPropertyName("tags")]
        public WhisperTags Tags { get; set; }

        [JsonPropertyName("recipient")]
        public WhisperRecipient Recipient { get; set; }

        [JsonPropertyName("nonce")]
        public string Nonce { get; set; }
    }

    public class WhisperRecipient
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }
    }

    public class WhisperTags
    {
        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("emotes")]
        public Emote[] Emotes { get; set; }

        [JsonPropertyName("badges")]
        public TwitchPubSubWhisperBadge[] Badges { get; set; }
    }

    public class TwitchPubSubWhisperBadge
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
}
