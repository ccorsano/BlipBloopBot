﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BlipBloopBot.Twitch.IRC
{
    public ref struct ParsedIRCMessage
    {
        public ReadOnlySpan<char> Prefix { get; internal set; }
        public ReadOnlySpan<char> Command { get; internal set; }
        public ReadOnlySpan<char> Param1 { get; internal set; }
        public ReadOnlySpan<char> Param2 { get; internal set; }
        public ReadOnlySpan<char> Param3 { get; internal set; }
        public ReadOnlySpan<char> Param4 { get; internal set; }
        public ReadOnlySpan<char> Param5 { get; internal set; }
        public ReadOnlySpan<char> Trailing { get; internal set; }
    }
}
