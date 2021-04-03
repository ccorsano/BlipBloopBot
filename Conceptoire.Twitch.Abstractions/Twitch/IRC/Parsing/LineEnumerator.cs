using System;

namespace BlipBloopBot.Twitch.IRC.Parsing
{
    public ref struct LineEnumerator
    {
        private ReadOnlySpan<char> _sourceStringSegment;

        public LineItem Current { get; private set; }

        public LineEnumerator(ReadOnlySpan<char> sourceString)
        {
            _sourceStringSegment = sourceString;
            Current = default;
        }

        public bool MoveNext()
        {
            var span = _sourceStringSegment;

            if (span.Length == 0)
                return false;

            var lineBreakIndex = span.IndexOf('\r');
            if (lineBreakIndex == -1)
            {
                _sourceStringSegment = ReadOnlySpan<char>.Empty;
                Current = new LineItem(span);
                return true;
            }
            else
            {
                if (lineBreakIndex == span.Length || span[lineBreakIndex + 1] != '\n')
                    return false;
            }

            Current = new LineItem(span.Slice(0, lineBreakIndex));
            _sourceStringSegment = span.Slice(lineBreakIndex + 2);
            return true;
        }

        public LineEnumerator GetEnumerator() => this;
    }
}
