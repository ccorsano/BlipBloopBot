using System;

namespace Conceptoire.Twitch.IRC.Parsing
{
    public ref struct TagItem
    {
        public ReadOnlySpan<char> Key { get; }
        public ReadOnlySpan<char> Value { get; }

        public TagItem(ReadOnlySpan<char> tag)
        {
            ReadOnlySpan<char> key, value;
            Parse(tag, out key, out value);
            Key = key;
            Value = value;

            void Parse(ReadOnlySpan<char> tag, out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)
            {
                var valueDelimiterIndex = tag.IndexOf('=');
                if (valueDelimiterIndex == -1)
                {
                    // Invalid !
                    key = ReadOnlySpan<char>.Empty;
                    value = ReadOnlySpan<char>.Empty;
                    return;
                }
                key = tag.Slice(0, valueDelimiterIndex);
                value = tag.Slice(valueDelimiterIndex + 1);
            }
        }

    }
}
