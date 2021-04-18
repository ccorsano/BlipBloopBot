using Conceptoire.Twitch.Constants;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.PubSub
{
    internal class TopicConverter : JsonConverter<Topic>
    {
        private const byte DOT = (byte) '.';

        public override Topic Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string firstScope;
            string secondScope;

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Expected a string token");
            }
            var firstDot = reader.ValueSpan.IndexOf(DOT);
            if (firstDot == -1)
            {
                throw new JsonException("Malformed topic, did not contain a dot");
            }
            var topicTypeStr = Encoding.UTF8.GetString(reader.ValueSpan.Slice(0, firstDot));
            var tail = reader.ValueSpan.Slice(firstDot + 1);
            var secondDot = tail.IndexOf(DOT);
            if (secondDot != -1)
            {
                firstScope = Encoding.UTF8.GetString(tail.Slice(0, secondDot));
                secondScope = Encoding.UTF8.GetString(tail.Slice(secondDot + 1));
            }
            else
            {
                firstScope = Encoding.UTF8.GetString(tail);
                secondScope = null;
            }

            return new Topic(TwitchConstants.GetTopicValue(topicTypeStr), firstScope, secondScope);
        }

        public override void Write(Utf8JsonWriter writer, Topic value, JsonSerializerOptions options)
        {
            var builder = new StringBuilder(TwitchConstants.GetTopicString(value.TopicType));
            builder.Append('.');
            builder.Append(value.Scope1);
            if (! string.IsNullOrEmpty(value.Scope2))
            {
                builder.Append('.');
                builder.Append(value.Scope2);
            }
            writer.WriteStringValue(builder.ToString());
        }
    }
}