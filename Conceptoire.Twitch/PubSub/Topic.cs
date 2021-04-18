using Conceptoire.Twitch.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.PubSub
{
    [JsonConverter(typeof(TopicConverter))]
    public readonly struct Topic
    {
        public Topic(TwitchConstants.PubSubTopicType pubSubTopic, string scope1)
        {
            TopicType = pubSubTopic;
            Scope1 = scope1;
            Scope2 = null;
        }

        public Topic(TwitchConstants.PubSubTopicType pubSubTopic, string scope1, string scope2)
        {
            TopicType = pubSubTopic;
            Scope1 = scope1;
            Scope2 = scope2;
        }

        public override string ToString()
        {
            if(Scope2 != null)
            {
                return $"{TwitchConstants.GetTopicString(TopicType)}.{Scope1}.{Scope2}";
            }
            else
            {
                return $"{TwitchConstants.GetTopicString(TopicType)}.{Scope1}";
            }
        }

        public readonly TwitchConstants.PubSubTopicType TopicType;
        public readonly string Scope1;
        public readonly string Scope2;
    }
}
