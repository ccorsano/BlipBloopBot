using Conceptoire.Twitch.IRC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Conceptoire.Twitch.Extensions
{
    public static class ParsedIRCMessageExtensions
    {
        public static string GetMessageIdTag(this ParsedIRCMessage message)
        {
            string msgId = null;
            foreach (var tag in message.ParseIRCTags())
            {
                if (tag.Key.SequenceEqual("id"))
                {
                    msgId = new string(tag.Value);
                    break;
                }
            }
            return msgId;
        }
    }
}
