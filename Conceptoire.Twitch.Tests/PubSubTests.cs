using Conceptoire.Twitch.Constants;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace Conceptoire.Twitch.Tests
{
    public class PubSubTests
    {
        public PubSubTests()
        {
        }

        public string GetEmbeddedSample(string sampleName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Conceptoire.Twitch.Tests.Samples.{sampleName}.json"))
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        [Fact]
        public void CanDeserializeResponse()
        {
            var sample = "{\"type\":\"RESPONSE\",\"nonce\":\"44h1k13746815ab1r2\",\"error\":\"\"}";

            var serverMessage = JsonSerializer.Deserialize<PubSub.ServerMessage>(sample);
            Assert.NotNull(serverMessage);
            Assert.Equal(TwitchConstants.PUBSUB_SERVER_RESPONSE, serverMessage.Type);
            Assert.Equal("44h1k13746815ab1r2", serverMessage.Nonce);
            Assert.Equal("", serverMessage.Error);
            Assert.Null(serverMessage.Data);
        }

        [Fact]
        public void CanDeserializeBitsEventV1Message()
        {
            var sample = GetEmbeddedSample("PubSub_BitsV1SampleFromDocs");
            //var sample = "{\"type\":\"MESSAGE\",\"data\":{\"topic\":\"channel-bits-events-v1.44322889\",\"message\":\"{\\\"data\\\":{\\\"user_name\\\":\\\"dallasnchains\\\",\\\"channel_name\\\":\\\"dallas\\\",\\\"user_id\\\":\\\"129454141\\\",\\\"channel_id\\\":\\\"44322889\\\",\\\"time\\\":\\\"2017-02-09T13:23:58.168Z\\\",\\\"chat_message\\\":\\\"cheer10000 New badge hype!\\\",\\\"bits_used\\\":10000,\\\"total_bits_used\\\":25000,\\\"context\\\":\\\"cheer\\\",\\\"badge_entitlement\\\":{\\\"new_version\\\":25000,\\\"previous_version\\\":10000}},\\\"version\\\":\\\"1.0\\\",\\\"message_type\\\":\\\"bits_event\\\",\\\"message_id\\\":\\\"8145728a4-35f0-4cf7-9dc0-f2ef24de1eb6\\\"}\"}}";

            var serverMessage = JsonSerializer.Deserialize<PubSub.ServerMessage>(sample);
            Assert.NotNull(serverMessage);
            Assert.Equal(TwitchConstants.PUBSUB_SERVER_MESSAGE, serverMessage.Type);
            Assert.Null(serverMessage.Nonce);
            Assert.Null(serverMessage.Error);
            Assert.NotNull(serverMessage.Data);
            Assert.Equal(TwitchConstants.PubSubTopicType.BitsV1, serverMessage.Data.Topic.TopicType);
            Assert.Equal("44322889", serverMessage.Data.Topic.Scope1);
            Assert.Null(serverMessage.Data.Topic.Scope2);
            Assert.IsType<PubSub.BitsEventV1>(serverMessage.Data.Message);
            var bitsEventV2Data = (PubSub.BitsEventV1)serverMessage.Data.Message;
            Assert.Equal("1.0", bitsEventV2Data.Version);
            Assert.Equal("bits_event", bitsEventV2Data.MessageType);
            Assert.Equal("8145728a4-35f0-4cf7-9dc0-f2ef24de1eb6", bitsEventV2Data.MessageId);
            Assert.Equal("dallasnchains", bitsEventV2Data.Data.UserName);
            Assert.Equal("dallas", bitsEventV2Data.Data.ChannelName);
            Assert.Equal("129454141", bitsEventV2Data.Data.UserId);
            Assert.Equal("44322889", bitsEventV2Data.Data.ChannelId);
            Assert.Equal(DateTimeOffset.Parse("2017-02-09T13:23:58.168Z"), bitsEventV2Data.Data.Time);
            Assert.Equal("cheer10000 New badge hype!", bitsEventV2Data.Data.ChatMessage);
            Assert.Equal(10000, bitsEventV2Data.Data.BitsUsed);
            Assert.Equal(25000, bitsEventV2Data.Data.TotalBitsUsed);
        }

        [Fact]
        public void CanDeserializeBitsEventV2Message()
        {
            var sample = GetEmbeddedSample("PubSub_BitsV2SampleFromDocs");
            //var sample = "{\"type\":\"MESSAGE\",\"data\":{\"topic\":\"channel-bits-events-v2.46024993\",\"message\":\"{\\\"data\\\":{\\\"user_name\\\":\\\"jwp\\\",\\\"channel_name\\\":\\\"bontakun\\\",\\\"user_id\\\":\\\"95546976\\\",\\\"channel_id\\\":\\\"46024993\\\",\\\"time\\\":\\\"2017-02-09T13:23:58.168Z\\\",\\\"chat_message\\\":\\\"cheer10000 New badge hype!\\\",\\\"bits_used\\\":10000,\\\"total_bits_used\\\":25000,\\\"context\\\":\\\"cheer\\\",\\\"badge_entitlement\\\":{\\\"new_version\\\":25000,\\\"previous_version\\\":10000}},\\\"version\\\":\\\"1.0\\\",\\\"message_type\\\":\\\"bits_event\\\",\\\"message_id\\\":\\\"8145728a4-35f0-4cf7-9dc0-f2ef24de1eb6\\\",\\\"is_anonymous\\\":true}\"}}";

            var serverMessage = JsonSerializer.Deserialize<PubSub.ServerMessage>(sample);
            Assert.NotNull(serverMessage);
            Assert.Equal(TwitchConstants.PUBSUB_SERVER_MESSAGE, serverMessage.Type);
            Assert.Null(serverMessage.Nonce);
            Assert.Null(serverMessage.Error);
            Assert.NotNull(serverMessage.Data);
            Assert.Equal(TwitchConstants.PubSubTopicType.BitsV2, serverMessage.Data.Topic.TopicType);
            Assert.Equal("46024993", serverMessage.Data.Topic.Scope1);
            Assert.Null(serverMessage.Data.Topic.Scope2);
            Assert.IsType<PubSub.BitsEventV2>(serverMessage.Data.Message);
            var bitsEventV2Data = (PubSub.BitsEventV2) serverMessage.Data.Message;
            Assert.Equal("1.0", bitsEventV2Data.Version);
            Assert.Equal("bits_event", bitsEventV2Data.MessageType);
            Assert.Equal("8145728a4-35f0-4cf7-9dc0-f2ef24de1eb6", bitsEventV2Data.MessageId);
            Assert.Equal("jwp", bitsEventV2Data.Data.UserName);
            Assert.Equal("bontakun", bitsEventV2Data.Data.ChannelName);
            Assert.Equal("95546976", bitsEventV2Data.Data.UserId);
            Assert.Equal("46024993", bitsEventV2Data.Data.ChannelId);
            Assert.Equal(DateTimeOffset.Parse("2017-02-09T13:23:58.168Z"), bitsEventV2Data.Data.Time);
            Assert.Equal("cheer10000 New badge hype!", bitsEventV2Data.Data.ChatMessage);
            Assert.Equal(10000, bitsEventV2Data.Data.BitsUsed);
            Assert.Equal(25000, bitsEventV2Data.Data.TotalBitsUsed);
            Assert.True(bitsEventV2Data.IsAnonymous);
        }

        [Fact]
        public void CanDeserializeBitsBadgeNotificationMessage()
        {
            var sample = GetEmbeddedSample("PubSub_BitsBadgeSampleFromDocs");
            //var sample = "{\"type\":\"MESSAGE\",\"data\":{\"topic\":\"channel-bits-badge-unlocks.401394874\",\"message\":\"     {          \\\"user_id\\\":\\\"232889822\\\",\\\"user_name\\\":\\\"willowolf\\\",\\\"channel_id\\\":\\\"401394874\\\",\\\"channel_name\\\":\\\"fun_test12345\\\",\\\"badge_tier\\\":1000,\\\"chat_message\\\":\\\"this should be received by the public pubsub listener\\\",\\\"time\\\":\\\"2020-12-06T00:01:43.71253159Z\\\"}\"}}";

            var serverMessage = JsonSerializer.Deserialize<PubSub.ServerMessage>(sample);
            Assert.NotNull(serverMessage);
            Assert.Equal(TwitchConstants.PUBSUB_SERVER_MESSAGE, serverMessage.Type);
            Assert.Null(serverMessage.Nonce);
            Assert.Null(serverMessage.Error);
            Assert.NotNull(serverMessage.Data);
            Assert.Equal(TwitchConstants.PubSubTopicType.BitsBadge, serverMessage.Data.Topic.TopicType);
            Assert.Equal("401394874", serverMessage.Data.Topic.Scope1);
            Assert.Null(serverMessage.Data.Topic.Scope2);
            Assert.IsType<PubSub.BitsBadge>(serverMessage.Data.Message);
            var bitsBadgeData = (PubSub.BitsBadge)serverMessage.Data.Message;
            Assert.Equal("232889822", bitsBadgeData.UserId);
            Assert.Equal("willowolf", bitsBadgeData.UserName);
            Assert.Equal("401394874", bitsBadgeData.ChannelId);
            Assert.Equal("fun_test12345", bitsBadgeData.ChannelName);
            Assert.Equal(1000, bitsBadgeData.BadgeTier);
            Assert.Equal("this should be received by the public pubsub listener", bitsBadgeData.ChatMessage);
            Assert.Equal(DateTimeOffset.Parse("2020-12-06T00:01:43.7125315Z"), bitsBadgeData.Time);
        }

        [Fact]
        public void CanDeserializeChannelSubscriptionsEventMessage()
        {
            var sample = "{\"type\":\"MESSAGE\",\"data\":{\"topic\":\"channel-points-channel-v1.30515034\",\"message\":\"{\\\"type\\\":\\\"reward-redeemed\\\",\\\"data\\\":{\\\"timestamp\\\":\\\"2019-11-12T01:29:34.98329743Z\\\",\\\"redemption\\\":{\\\"id\\\":\\\"9203c6f0-51b6-4d1d-a9ae-8eafdb0d6d47\\\",\\\"user\\\":{\\\"id\\\":\\\"30515034\\\",\\\"login\\\":\\\"davethecust\\\",\\\"display_name\\\":\\\"davethecust\\\"},\\\"channel_id\\\":\\\"30515034\\\",\\\"redeemed_at\\\":\\\"2019-12-11T18:52:53.128421623Z\\\",\\\"reward\\\":{\\\"id\\\":\\\"6ef17bb2-e5ae-432e-8b3f-5ac4dd774668\\\",\\\"channel_id\\\":\\\"30515034\\\",\\\"title\\\":\\\"hit a gleesh walk on stream\\\",\\\"prompt\\\":\\\"cleanside's finest \\\\n\\\",\\\"cost\\\":10,\\\"is_user_input_required\\\":true,\\\"is_sub_only\\\":false,\\\"image\\\":{\\\"url_1x\\\":\\\"https://static-cdn.jtvnw.net/custom-reward-images/30515034/6ef17bb2-e5ae-432e-8b3f-5ac4dd774668/7bcd9ca8-da17-42c9-800a-2f08832e5d4b/custom-1.png\\\",\\\"url_2x\\\":\\\"https://static-cdn.jtvnw.net/custom-reward-images/30515034/6ef17bb2-e5ae-432e-8b3f-5ac4dd774668/7bcd9ca8-da17-42c9-800a-2f08832e5d4b/custom-2.png\\\",\\\"url_4x\\\":\\\"https://static-cdn.jtvnw.net/custom-reward-images/30515034/6ef17bb2-e5ae-432e-8b3f-5ac4dd774668/7bcd9ca8-da17-42c9-800a-2f08832e5d4b/custom-4.png\\\"},\\\"default_image\\\":{\\\"url_1x\\\":\\\"https://static-cdn.jtvnw.net/custom-reward-images/default-1.png\\\",\\\"url_2x\\\":\\\"https://static-cdn.jtvnw.net/custom-reward-images/default-2.png\\\",\\\"url_4x\\\":\\\"https://static-cdn.jtvnw.net/custom-reward-images/default-4.png\\\"},\\\"background_color\\\":\\\"#00C7AC\\\",\\\"is_enabled\\\":true,\\\"is_paused\\\":false,\\\"is_in_stock\\\":true,\\\"max_per_stream\\\":{\\\"is_enabled\\\":false,\\\"max_per_stream\\\":0},\\\"should_redemptions_skip_request_queue\\\":true},\\\"user_input\\\":\\\"yeooo\\\",\\\"status\\\":\\\"FULFILLED\\\"}}}\"}}";

            var serverMessage = JsonSerializer.Deserialize<PubSub.ServerMessage>(sample);
            Assert.NotNull(serverMessage);
            Assert.Equal(TwitchConstants.PUBSUB_SERVER_MESSAGE, serverMessage.Type);
            Assert.Null(serverMessage.Nonce);
            Assert.Null(serverMessage.Error);
            Assert.NotNull(serverMessage.Data);
            Assert.Equal(TwitchConstants.PubSubTopicType.ChannelPointsV1, serverMessage.Data.Topic.TopicType);
            Assert.Equal("30515034", serverMessage.Data.Topic.Scope1);
            Assert.Null(serverMessage.Data.Topic.Scope2);
            Assert.IsType<PubSub.ChannelPointsV1>(serverMessage.Data.Message);
            var channelPointData = (PubSub.ChannelPointsV1) serverMessage.Data.Message;
            Assert.Equal("reward-redeemed", channelPointData.Type);
            Assert.Equal(DateTimeOffset.Parse("2019-11-12T01:29:34.98329743Z"), channelPointData.Data.Timestamp);
            Assert.Equal(Guid.Parse("9203c6f0-51b6-4d1d-a9ae-8eafdb0d6d47"), channelPointData.Data.Redemption.Id);
            Assert.Equal("30515034", channelPointData.Data.Redemption.User.Id);
            Assert.Equal("davethecust", channelPointData.Data.Redemption.User.Login);
            Assert.Equal("davethecust", channelPointData.Data.Redemption.User.DisplayName);
            Assert.Equal("30515034", channelPointData.Data.Redemption.ChannelId);
            Assert.Equal(DateTimeOffset.Parse("2019-12-11T18:52:53.128421623Z"), channelPointData.Data.Redemption.RedeemedAt);

            Assert.Equal(Guid.Parse("6ef17bb2-e5ae-432e-8b3f-5ac4dd774668"), channelPointData.Data.Redemption.Reward.Id);
            Assert.Equal("30515034", channelPointData.Data.Redemption.Reward.ChannelId);
            Assert.Equal("hit a gleesh walk on stream", channelPointData.Data.Redemption.Reward.Title);
            Assert.Equal("cleanside's finest \n", channelPointData.Data.Redemption.Reward.Prompt);
            Assert.Equal(10, channelPointData.Data.Redemption.Reward.Cost);
            Assert.True(channelPointData.Data.Redemption.Reward.IsUserInputRequired);
            Assert.False(channelPointData.Data.Redemption.Reward.IsSubOnly);
            Assert.True(channelPointData.Data.Redemption.Reward.IsInStock);
            Assert.True(channelPointData.Data.Redemption.Reward.IsEnabled);
            Assert.False(channelPointData.Data.Redemption.Reward.IsPaused);
            Assert.True(channelPointData.Data.Redemption.Reward.ShouldRedemptionsSkipRequestQueue);
            Assert.False(channelPointData.Data.Redemption.Reward.MaxPerStream.IsEnabled);
            Assert.Equal(0, channelPointData.Data.Redemption.Reward.MaxPerStream.MaxPerStream);
            Assert.Equal("#00C7AC", channelPointData.Data.Redemption.Reward.BackgroundColor);
            Assert.Equal("yeooo", channelPointData.Data.Redemption.UserInput);
            Assert.Equal("FULFILLED", channelPointData.Data.Redemption.Status);
        }

        [Fact]
        public void CanDeserializeWhisperMessage()
        {
            var sample = GetEmbeddedSample("PubSub_WhisperSample");

            var serverMessage = JsonSerializer.Deserialize<PubSub.ServerMessage>(sample);
            Assert.NotNull(serverMessage);
            Assert.Equal(TwitchConstants.PUBSUB_SERVER_MESSAGE, serverMessage.Type);
            Assert.Null(serverMessage.Nonce);
            Assert.Null(serverMessage.Error);
            Assert.NotNull(serverMessage.Data);
            Assert.Equal(TwitchConstants.PubSubTopicType.Whispers, serverMessage.Data.Topic.TopicType);
            Assert.Equal("158511925", serverMessage.Data.Topic.Scope1);
            Assert.Null(serverMessage.Data.Topic.Scope2);
            Assert.IsType<PubSub.WhisperEvent>(serverMessage.Data.Message);
            var whisperData = (PubSub.WhisperEvent) serverMessage.Data.Message;
            Assert.Equal("whisper_received", whisperData.Type);
            Assert.Equal(6, whisperData.DataObject.Id);
            Assert.Equal(Guid.Parse("e47d4b51-db09-4184-89b7-fd58927c16af"), whisperData.DataObject.MessageId);
            Assert.Equal("test VoHiYo SmoocherZ cmonBruh KomodoHype", whisperData.DataObject.Body);
            Assert.Equal(660015310, whisperData.DataObject.FromId);
            Assert.Equal(Guid.Parse("e47d4b51-db09-4184-89b7-fd58927c16af"), whisperData.DataObject.MessageId);
        }
    }
}
