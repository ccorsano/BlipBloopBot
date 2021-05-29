using Conceptoire.Twitch.IRC.Parsing;
using Xunit;

namespace BlipBloopBotTests
{
    public class TestIRCMessageParsing
    {
        [Fact]
        public void CanParseMessageWithTags()
        {
            var sourceString = "@badge-info=;badges=broadcaster/1;client-nonce=8349a9d6e0dc4f397ae73e9a52633ea0;color=#9ACD32;display-name=Miekyld;emotes=;flags=;id=76694b90-c8ea-4175-be7b-a11685605d20;mod=0;room-id=158511925;subscriber=0;tmi-sent-ts=1616545324227;turbo=0;user-id=158511925;user-type= :miekyld!miekyld@miekyld.tmi.twitch.tv PRIVMSG #miekyld :!jeu";
            var lineItem = new LineItem(sourceString);
            Assert.Equal("PRIVMSG", new string(lineItem.Message.Command));
            Assert.Equal("!jeu", new string(lineItem.Message.Trailing));
            Assert.Equal("#miekyld", new string(lineItem.Message.Param1));
            Assert.Equal("miekyld!miekyld@miekyld.tmi.twitch.tv", new string(lineItem.Message.Prefix));
            Assert.Equal("badge-info=;badges=broadcaster/1;client-nonce=8349a9d6e0dc4f397ae73e9a52633ea0;color=#9ACD32;display-name=Miekyld;emotes=;flags=;id=76694b90-c8ea-4175-be7b-a11685605d20;mod=0;room-id=158511925;subscriber=0;tmi-sent-ts=1616545324227;turbo=0;user-id=158511925;user-type=", new string(lineItem.Message.Tags));
        }

        [Fact]
        public void CanParseTags()
        {
            var sourceString = "@badge-info=;badges=broadcaster/1;client-nonce=8349a9d6e0dc4f397ae73e9a52633ea0;color=#9ACD32;display-name=Miekyld;emotes=;flags=;id=76694b90-c8ea-4175-be7b-a11685605d20;mod=0;room-id=158511925;subscriber=0;tmi-sent-ts=1616545324227;turbo=0;user-id=158511925;user-type= :miekyld!miekyld@miekyld.tmi.twitch.tv PRIVMSG #miekyld :!jeu";
            var lineItem = new LineItem(sourceString);
            Assert.Equal("badge-info=;badges=broadcaster/1;client-nonce=8349a9d6e0dc4f397ae73e9a52633ea0;color=#9ACD32;display-name=Miekyld;emotes=;flags=;id=76694b90-c8ea-4175-be7b-a11685605d20;mod=0;room-id=158511925;subscriber=0;tmi-sent-ts=1616545324227;turbo=0;user-id=158511925;user-type=", new string(lineItem.Message.Tags));
            var tagsEnumerator = new TagsEnumerator(lineItem.Message.Tags);
            Assert.True(tagsEnumerator.MoveNext());
            var firstTag = tagsEnumerator.Current;
            Assert.Equal("badge-info", new string(firstTag.Key));
            Assert.Equal(string.Empty, new string(firstTag.Value));
            Assert.True(tagsEnumerator.MoveNext());
            var secondTag = tagsEnumerator.Current;
            Assert.Equal("badges", new string(secondTag.Key));
            Assert.Equal("broadcaster/1", new string(secondTag.Value));
            Assert.True(tagsEnumerator.MoveNext());
            var thirdTag = tagsEnumerator.Current;
            Assert.Equal("client-nonce", new string(thirdTag.Key));
            Assert.Equal("8349a9d6e0dc4f397ae73e9a52633ea0", new string(thirdTag.Value));
            Assert.True(tagsEnumerator.MoveNext());
            var fourthTag = tagsEnumerator.Current;
            Assert.Equal("color", new string(fourthTag.Key));
            Assert.Equal("#9ACD32", new string(fourthTag.Value));
            Assert.True(tagsEnumerator.MoveNext());
            var fifthTag = tagsEnumerator.Current;
            Assert.Equal("display-name", new string(fifthTag.Key));
            Assert.Equal("Miekyld", new string(fifthTag.Value));
            Assert.True(tagsEnumerator.MoveNext());
            // emotes=
            Assert.True(tagsEnumerator.MoveNext());
            // flags=
            Assert.True(tagsEnumerator.MoveNext());
            var idTag = tagsEnumerator.Current;
            Assert.Equal("id", new string(idTag.Key));
            Assert.Equal("76694b90-c8ea-4175-be7b-a11685605d20", new string(idTag.Value));
            Assert.True(tagsEnumerator.MoveNext());
            // mod=0
            Assert.True(tagsEnumerator.MoveNext());
            // room-id=158511925
            Assert.True(tagsEnumerator.MoveNext());
            // subscriber=0
            Assert.True(tagsEnumerator.MoveNext());
            // tmi-sent-ts=1616545324227
            Assert.True(tagsEnumerator.MoveNext());
            // turbo=0
            Assert.True(tagsEnumerator.MoveNext());
            // user-id=158511925
            Assert.True(tagsEnumerator.MoveNext());
            // user-type=
            Assert.False(tagsEnumerator.MoveNext());
        }
    }
}
