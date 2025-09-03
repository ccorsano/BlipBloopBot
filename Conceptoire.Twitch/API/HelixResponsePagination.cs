using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    /// <summary>
    /// Custom converter for pagination markers
    /// Covers some of the Twitch API where pagination is not wrapped in a "cursor" object
    /// , but the cursor value is directly passed as the value of "pagination"
    /// </summary>
    public class HelixResponsePaginationConverter : JsonConverter<HelixResponsePagination>
    {
        public override HelixResponsePagination Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return new HelixResponsePagination
                {
                    Cursor = System.Text.Encoding.UTF8.GetString(reader.ValueSpan)
                };
            }
            return new HelixResponsePagination(JsonSerializer.Deserialize<HelixResponsePaginationInner>(ref reader, HelixResponsePaginationInnerContext.Default.HelixResponsePaginationInner));
        }

        public override void Write(Utf8JsonWriter writer, HelixResponsePagination value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, HelixResponsePaginationInnerContext.Default.HelixResponsePaginationInner);
        }
    }

    [JsonConverter(typeof(HelixResponsePaginationConverter))]
    public class HelixResponsePagination
    {
        public HelixResponsePagination() {}

        internal HelixResponsePagination(HelixResponsePaginationInner inner)
        {
            Cursor = inner.Cursor;
        }

        public string Cursor { get; set; }
    }

    internal class HelixResponsePaginationInner
    {
        [JsonPropertyName("cursor")]
        public string Cursor { get; set; }
    }

    [JsonSerializable(typeof(HelixResponsePaginationInner))]
    internal partial class HelixResponsePaginationInnerContext : JsonSerializerContext { }
}
