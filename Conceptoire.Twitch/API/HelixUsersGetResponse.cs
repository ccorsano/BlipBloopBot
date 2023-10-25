using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Conceptoire.Twitch.API
{
    public class HelixUsersGetResponse
    {
        [JsonPropertyName("data")]
        public HelixUsersGetResult[] Data { get; set; }
    }

    [JsonSerializable(typeof(HelixUsersGetResponse))]
    internal partial class HelixUsersGetResponseJsonContext : JsonSerializerContext { }
}