using Conceptoire.Twitch.Constants;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.PubSub
{
    public class MessageDataConverter : JsonConverter<MessageData>
    {
        private ILogger _logger;

        public MessageDataConverter()
        {
        }

        public override MessageData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            MessageData messageData = new MessageData();
            if (! reader.Read() || reader.TokenType != JsonTokenType.PropertyName || !reader.ValueTextEquals("topic"))
            {
                throw new JsonException("Unexpected token, was expecting 'topic' property");
            }
            if (!reader.Read() || reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Unexpected token, was expecting 'topic' value string");
            }
            messageData.Topic = JsonSerializer.Deserialize<Topic>(ref reader);

            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || !reader.ValueTextEquals("message"))
            {
                throw new JsonException("Unexpected token, was expecting 'message' property");
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Unexpected token, was expecting 'message' json string");
            }

            switch (messageData.Topic.TopicType)
            {
                case TwitchConstants.PubSubTopicType.BitsV1:
                    messageData.Message = JsonSerializer.Deserialize<BitsEventV1>(reader.GetString());
                    break;
                case TwitchConstants.PubSubTopicType.BitsV2:
                    messageData.Message = JsonSerializer.Deserialize<BitsEventV2>(reader.GetString());
                    break;
                case TwitchConstants.PubSubTopicType.BitsBadge:
                    messageData.Message = JsonSerializer.Deserialize<BitsBadge>(reader.GetString());
                    break;
                case TwitchConstants.PubSubTopicType.ChannelPointsV1:
                    messageData.Message = JsonSerializer.Deserialize<ChannelPointsV1>(reader.GetString());
                    break;
                case TwitchConstants.PubSubTopicType.ChannelSubscriptionsV1:
                    messageData.Message = JsonSerializer.Deserialize<SubscriptionEventV1>(reader.GetString());
                    break;
                case TwitchConstants.PubSubTopicType.ChatModeration:
                    //_logger.LogError("Received a PubSub ChatModeration message {chatModerationMessage}", reader.GetString());
                    throw new NotImplementedException($"Needed a example of ChatModeration event, here is one: {reader.GetString()}");
                //messageData.Message = JsonSerializer.Deserialize<ChatModerationEvent>(reader.ValueSpan);
                //break;
                case TwitchConstants.PubSubTopicType.Whispers:
                    messageData.Message = JsonSerializer.Deserialize<BitsEventV1>(reader.GetString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported topic {TwitchConstants.GetTopicString(messageData.Topic.TopicType)}");
            }

            // Skip any extra data
            while(reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                _logger.LogWarning("PubSub message contained extra JSON token {jsonTokenType}", reader.TokenType);
            }

            return messageData;
        }

        public override void Write(Utf8JsonWriter writer, MessageData value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value", "Null MessageData value is not supported");
            }
            writer.WritePropertyName("topic");
            JsonSerializer.Serialize(value.Topic);
            writer.WritePropertyName("message");
            throw new NotImplementedException();
        }
    }
}
