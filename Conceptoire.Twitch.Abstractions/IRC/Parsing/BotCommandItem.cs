using System;

namespace Conceptoire.Twitch.IRC.Parsing
{
    public ref struct BotCommandItem
    {
        public BotCommandItem(ReadOnlySpan<char> cmd)
        {
            _cmd = cmd;
        }

        public ReadOnlySpan<char> _cmd;

        public static implicit operator string(BotCommandItem entry) => new string(entry._cmd);
    }
}
