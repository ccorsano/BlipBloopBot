using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.PubSub
{
    [JsonConverter(typeof(MessageDataConverter))]
    public class MessageData
    {
        public Topic Topic { get; set; }

        public IPubSubDataObject Message { get; set; }
    }
}
