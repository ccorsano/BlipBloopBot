﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.PubSub
{
    public class TwitchPubSubWhisperEvent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("data")]
        public TwitchPubSubWhisperData Data { get; set; }
    }

    public class TwitchPubSubWhisperData
    {
        [JsonPropertyName("topic")]
        public string Topic { get; set; }

        [JsonPropertyName("message")]
        public TwitchPubSubWhisperDataObject Message { get; set; }

        [JsonPropertyName("data_object")]
        public TwitchPubSubWhisperDataObject DataObject { get; set; }
    }

    public class TwitchPubSubWhisperDataObject
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("thread_id")]
        public string ThreadId { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("sent_ts")]
        public long SentTs { get; set; }

        [JsonPropertyName("from_id")]
        public long FromId { get; set; }

        [JsonPropertyName("tags")]
        public TwitchPubSubWhisperTags Tags { get; set; }

        [JsonPropertyName("recipient")]
        public TwitchPubSubWhisperRecipient Recipient { get; set; }

        [JsonPropertyName("nonce")]
        public string Nonce { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("data")]
        public TwitchPubSubWhisperDataObjectData Data { get; set; }
    }

    public class TwitchPubSubWhisperDataObjectData
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }

    public class TwitchPubSubWhisperRecipient
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("badges")]
        public object[] Badges { get; set; }
    }

    public class TwitchPubSubWhisperTags
    {
        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("emotes")]
        public object[] Emotes { get; set; }

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
