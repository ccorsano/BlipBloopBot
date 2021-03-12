using BlipBloopBot.Twitch.IRC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BlipBloopBotTests
{
    public class TestBotCommandParsing
    {
        [Fact]
        public void CanParseString()
        {
            var sourceString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            var enumerator = sourceString.ParseBotCommands('!');
            var hasNext = enumerator.MoveNext();
            Assert.False(hasNext);
        }

        [Fact]
        public void CanParseSingleCommand()
        {
            var sourceString = "Lorem !ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            var enumerator = sourceString.ParseBotCommands('!');
            var hasNext = enumerator.MoveNext();
            Assert.True(hasNext);
            Assert.Equal("ipsum", new string(enumerator.Current));
            hasNext = enumerator.MoveNext();
            Assert.False(hasNext);
        }

        [Fact]
        public void CanParseCommandStartingMessage()
        {
            var sourceString = "!Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            var enumerator = sourceString.ParseBotCommands('!');
            var hasNext = enumerator.MoveNext();
            Assert.True(hasNext);
            Assert.Equal("Lorem", new string(enumerator.Current));
            hasNext = enumerator.MoveNext();
            Assert.False(hasNext);
        }

        [Fact]
        public void CanParseCommandEndingMessage()
        {
            var sourceString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est !laborum";
            var enumerator = sourceString.ParseBotCommands('!');
            var hasNext = enumerator.MoveNext();
            Assert.True(hasNext);
            Assert.Equal("laborum", new string(enumerator.Current));
            hasNext = enumerator.MoveNext();
            Assert.False(hasNext);
        }

        [Fact]
        public void CanParseCommandMessage()
        {
            var sourceString = "!laborum";
            var enumerator = sourceString.ParseBotCommands('!');
            var hasNext = enumerator.MoveNext();
            Assert.True(hasNext);
            Assert.Equal("laborum", new string(enumerator.Current));
            hasNext = enumerator.MoveNext();
            Assert.False(hasNext);
        }

        [Fact]
        public void CanParseBotCommandChar()
        {
            var sourceString = "!!!!!!!!!!";
            var enumerator = sourceString.ParseBotCommands('!');
            var hasNext = enumerator.MoveNext();
            Assert.False(hasNext);
            sourceString = "!";
            enumerator = sourceString.ParseBotCommands('!');
            hasNext = enumerator.MoveNext();
            Assert.False(hasNext);
            sourceString = "! ";
            enumerator = sourceString.ParseBotCommands('!');
            hasNext = enumerator.MoveNext();
            Assert.False(hasNext);
            sourceString = " !";
            enumerator = sourceString.ParseBotCommands('!');
            hasNext = enumerator.MoveNext();
            Assert.False(hasNext);
        }

        [Fact]
        public void CanParseManyCommands()
        {
            var sourceString = "Lorem !ipsum dolor !sit amet, consectetur adipiscing !elit, sed !do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id !est laborum.";
            var enumerator = sourceString.ParseBotCommands('!');
            var hasNext = enumerator.MoveNext();
            Assert.True(hasNext);
            Assert.Equal("ipsum", new string(enumerator.Current));
            hasNext = enumerator.MoveNext();
            Assert.True(hasNext);
            Assert.Equal("sit", new string(enumerator.Current));
            hasNext = enumerator.MoveNext();
            Assert.True(hasNext);
            Assert.Equal("elit", new string(enumerator.Current));
            hasNext = enumerator.MoveNext();
            Assert.True(hasNext);
            Assert.Equal("do", new string(enumerator.Current));
            hasNext = enumerator.MoveNext();
            Assert.True(hasNext);
            Assert.Equal("est", new string(enumerator.Current));
            hasNext = enumerator.MoveNext();
            Assert.False(hasNext);
        }
    }
}
