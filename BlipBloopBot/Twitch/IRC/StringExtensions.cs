using BlipBloopBot.Twitch.IRC.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlipBloopBot.Twitch.IRC
{
    /// <summary>
    /// String parsing extensions for IRC message processing
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Enumerate over lines in a given input string
        /// </summary>
        /// <param name="source">Raw IRC string</param>
        /// <returns></returns>
        public static LineEnumerator SplitLines(this string source)
        {
            return new LineEnumerator(source.AsSpan());
        }

        /// <summary>
        /// Enumerate possible bot commands in an input string
        /// </summary>
        /// <param name="message">Raw message string</param>
        /// <param name="commandSymbol">Prefix character for the bot commands, eg. '!'</param>
        /// <returns></returns>
        public static BotCommandEnumerator ParseBotCommands(this string message, char commandSymbol)
        {
            return new BotCommandEnumerator(message.AsSpan(), commandSymbol);
        }

        /// <summary>
        /// Enumerate possible bot commands in an input string
        /// </summary>
        /// <param name="message">Raw message string</param>
        /// <param name="commandSymbol">Prefix character for the bot commands, eg. '!'</param>
        /// <returns></returns>
        public static BotCommandEnumerator ParseBotCommands(this ReadOnlySpan<char> message, char commandSymbol)
        {
            return new BotCommandEnumerator(message, commandSymbol);
        }
    }
}
