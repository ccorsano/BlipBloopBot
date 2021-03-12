using System;

namespace BlipBloopBot.Twitch.IRC.Parsing
{
    public ref struct LineItem
    {
        private ReadOnlySpan<char> _line;
        public ParsedIRCMessage Message { get; }

        public LineItem(ReadOnlySpan<char> line)
        {
            ReadOnlySpan<char> prefix = default,
                command = default,
                param1 = default,
                param2 = default,
                param3 = default,
                param4 = default,
                param5 = default,
                trailing = default;

            _line = line;

            if (_line[0] == ':')
            {
                var commandIndex = _line.IndexOf(' ');
                prefix = _line.Slice(1, commandIndex - 1);
                _line = _line.Slice(commandIndex + 1);
            }

            // Command
            ParseSegment(ref _line, out command);

            // Param 1
            if (!_line.IsEmpty && _line[0] != ':')
                ParseSegment(ref _line, out param1);

            // Param 2
            if (!_line.IsEmpty && _line[0] != ':')
                ParseSegment(ref _line, out param2);

            // Param 3
            if (!_line.IsEmpty && _line[0] != ':')
                ParseSegment(ref _line, out param3);

            // Param 4
            if (!_line.IsEmpty && _line[0] != ':')
                ParseSegment(ref _line, out param4);

            // Param 5
            if (!_line.IsEmpty && _line[0] != ':')
                ParseSegment(ref _line, out param5);

            if (_line[0] == ':')
            {
                trailing = _line.Slice(1);
            }

            Message = new ParsedIRCMessage
            {
                Prefix = prefix,
                Command = command,
                Param1 = param1,
                Param2 = param2,
                Param3 = param3,
                Param4 = param4,
                Param5 = param5,
                Trailing = trailing
            };

            void ParseSegment(ref ReadOnlySpan<char> line, out ReadOnlySpan<char> target)
            {
                var endOfCommandIndex = line.IndexOf(' ');
                if (endOfCommandIndex == -1)
                {
                    target = ReadOnlySpan<char>.Empty;
                    return;
                }
                target = line.Slice(0, endOfCommandIndex);
                line = line.Slice(endOfCommandIndex + 1);
            }
        }

        public static implicit operator ReadOnlySpan<char>(LineItem entry) => entry._line;
    }
}
