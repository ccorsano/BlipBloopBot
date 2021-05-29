using System.Text.Json.Serialization;

namespace Conceptoire.Twitch.API
{
    public class HelixResponsePagination
    {
        [JsonPropertyName("cursor")]
        public string Cursor { get; set; }
    }
}
