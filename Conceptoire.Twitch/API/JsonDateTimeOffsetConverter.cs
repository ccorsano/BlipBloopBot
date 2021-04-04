using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class JsonDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
    {
        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            if (reader.TryGetDateTimeOffset(out var dateTimeOffset))
            {
                return dateTimeOffset;
            }
            else if (string.IsNullOrEmpty(reader.GetString()))
            {
                return null;
            }
            else
            {
                throw new JsonException();
            }
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
