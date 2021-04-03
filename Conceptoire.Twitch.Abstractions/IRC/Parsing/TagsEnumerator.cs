using System;

namespace Conceptoire.Twitch.IRC.Parsing
{
    public ref struct TagsEnumerator
    {
        private ReadOnlySpan<char> _sourceStringSegment;

        public TagItem Current { get; private set; }

        public TagsEnumerator(ReadOnlySpan<char> sourceString)
        {
            _sourceStringSegment = sourceString;
            Current = default;
        }

        public bool MoveNext()
        {
            var span = _sourceStringSegment;

            if (span.Length == 0)
                return false;

            var tagDelimiterIndex = span.IndexOf(';');
            if (tagDelimiterIndex == -1)
            {
                _sourceStringSegment = ReadOnlySpan<char>.Empty;
                Current = new TagItem(span);
                return true;
            }
            else
            {
                if (tagDelimiterIndex == span.Length)
                    return false;
            }

            Current = new TagItem(span.Slice(0, tagDelimiterIndex));
            _sourceStringSegment = span.Slice(tagDelimiterIndex + 1);
            return true;
        }

        public TagsEnumerator GetEnumerator() => this;
    }
}
