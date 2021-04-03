using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.IRC.Parsing
{
    /// <summary>
    /// Splitter struct to parse and iterate bot commands in a message string
    /// </summary>
    public ref struct BotCommandEnumerator
    {
        private readonly char _botPrefix;
        private ReadOnlySpan<char> _sourceStringSegment;

        public BotCommandItem Current { get; private set; }

        public BotCommandEnumerator(ReadOnlySpan<char> sourceString, char botPrefix)
        {
            _sourceStringSegment = sourceString;
            _botPrefix = botPrefix;
            Current = default;
        }

        public bool MoveNext()
        {
            var span = _sourceStringSegment;

            // Empty string
            if (span.Length == 0)
                return false;

            // Look for bot command prefix index (typically '!')
            var commandIndex = span.IndexOf(_botPrefix);
            // No command in there
            if (commandIndex == -1)
            {
                return false;
            }

            // Skip the first part of the message and the bot prefix char
            span = span.Slice(commandIndex + 1);

            // Find the end of the command word
            var endWordIndex = 0;
            while(endWordIndex < span.Length && char.IsLetterOrDigit(span[endWordIndex]))
            {
                ++endWordIndex;
            }

            // Null command or just an isolated prefix char, recurse to look for next command
            if (endWordIndex == 0)
            {
                _sourceStringSegment = span;
                return MoveNext();
            }

            if (endWordIndex == span.Length)
            {
                Current = new BotCommandItem(span);
                _sourceStringSegment = ReadOnlySpan<char>.Empty;
                return true;
            }

            Current = new BotCommandItem(span.Slice(0, endWordIndex));
            _sourceStringSegment = span.Slice(endWordIndex + 1);
            return true;
        }

        public BotCommandEnumerator GetEnumerator() => this;
    }
}
