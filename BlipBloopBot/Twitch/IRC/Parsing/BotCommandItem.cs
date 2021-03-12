using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.IRC.Parsing
{
    public ref struct BotCommandItem
    {
        public BotCommandItem(ReadOnlySpan<char> cmd)
        {
            _cmd = cmd;
        }

        public ReadOnlySpan<char> _cmd;

        public static implicit operator ReadOnlySpan<char>(BotCommandItem entry) => entry._cmd;
    }
}
