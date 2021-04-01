using BlipBloopBot.Constants;
using BlipBloopBot.Model.EventSub;
using BlipBloopBot.Options;
using BlipBloopBot.Twitch.EventSub;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BlipBloopBotTests
{
    public class TestEventSubNotifications
    {
        [Fact]
        public async Task RejectsRequestsWithMissingHeaders()
        {
            const string twitchNotification = @"{""data"":[{""id"":""f1c2a387-161a-49f9-a165-0f21d7a4e1c4"",""status"":""webhook_callback_verification_pending"",""type"":""channel.follow"",""version"":""1"",""cost"":1,""condition"":{""broadcaster_user_id"":""12826""},""transport"":{""method"":""webhook"",""callback"":""https://example.com/webhooks/callback""},""created_at"":""2019-11-16T10:11:12.123Z""}],""total"":1,""total_cost"":1,""max_total_cost"":10000,""limit"":10000}";
            using Stream requestBody = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(twitchNotification));

            var missingSignature = new Dictionary<string, StringValues>
            {
                { TwitchConstants.EventSubHeaderNames.MessageId, new StringValues("e76c6bd4-55c9-4987-8304-da1588d8988b") },
                { TwitchConstants.EventSubHeaderNames.MessageRetry, new StringValues("0") },
                { TwitchConstants.EventSubHeaderNames.MessageTimeStamp, new StringValues("2019-11-16T10:11:12.123Z") },
                { TwitchConstants.EventSubHeaderNames.MessageType, new StringValues("webhook_callback_verification") },
                { TwitchConstants.EventSubHeaderNames.SubscriptionType, new StringValues("channel.follow") },
                { TwitchConstants.EventSubHeaderNames.SubscriptionVersion, new StringValues("1") },
            };
            var missingMessageType = new Dictionary<string, StringValues>
            {
                { TwitchConstants.EventSubHeaderNames.MessageId, new StringValues("e76c6bd4-55c9-4987-8304-da1588d8988b") },
                { TwitchConstants.EventSubHeaderNames.MessageRetry, new StringValues("0") },
                { TwitchConstants.EventSubHeaderNames.MessageSignature, new StringValues("sha256=f56bf6ce06a1adf46fa27831d7d15d") },
                { TwitchConstants.EventSubHeaderNames.MessageTimeStamp, new StringValues("2019-11-16T10:11:12.123Z") },
                { TwitchConstants.EventSubHeaderNames.SubscriptionType, new StringValues("channel.follow") },
                { TwitchConstants.EventSubHeaderNames.SubscriptionVersion, new StringValues("1") },
            };

            Mock<HttpResponse> httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupSet(r => r.StatusCode = 403);

            Mock<HttpContext> httpContextMoqNoSignature = new Mock<HttpContext>();
            httpContextMoqNoSignature.Setup(c => c.Request.Headers)
                .Returns(new HeaderDictionary(missingSignature));
            httpContextMoqNoSignature.Setup(c => c.Request.Body)
                .Returns(requestBody);
            httpContextMoqNoSignature.Setup(c => c.Response)
                .Returns(httpResponse.Object);

            Mock<HttpContext> httpContextMoqNoMessageType = new Mock<HttpContext>();
            httpContextMoqNoMessageType.Setup(c => c.Request.Headers)
                .Returns(new HeaderDictionary(missingMessageType));
            httpContextMoqNoMessageType.Setup(c => c.Request.Body)
                .Returns(requestBody);
            httpContextMoqNoMessageType.Setup(c => c.Response)
                .Returns(httpResponse.Object);

            Func<EventSubContext, TwitchEventSubEvent, Task> assertingHandler = (context, twitchEvent) =>
            {
                throw new Exception("Should not reach the handler callback");
            };

            var eventSubOptions = new EventSubOptions
            {
                WebHookSecret = "secret",
            };

            var mockLogger = new Mock<ILogger<EventSubHandler>>();
            var eventHandler = new HandlerRegistration<TwitchEventSubEvent>(assertingHandler);

            var eventSubHandler = new EventSubHandler(
                new List<IHandlerRegistration> { eventHandler },
                new OptionsWrapper<EventSubOptions>(eventSubOptions),
                mockLogger.Object);

            await eventSubHandler.HandleRequestAsync(httpContextMoqNoSignature.Object);

            await eventSubHandler.HandleRequestAsync(httpContextMoqNoMessageType.Object);
        }

        [Fact]
        public async Task CanReceiveChannelUpdateNotification()
        {
            const string twitchNotification = @"{""subscription"":{""id"":""f1c2a387-161a-49f9-a165-0f21d7a4e1c4"",""type"":""channel.update"",""version"":""1"",""status"":""enabled"",""cost"":0,""condition"":{""broadcaster_user_id"":""1337""},""transport"":{""method"":""webhook"",""callback"":""https://example.com/webhooks/callback""},""created_at"":""2019-11-16T10:11:12.123Z""},""event"":{""broadcaster_user_id"":""1337"",""broadcaster_user_login"":""cool_user"",""broadcaster_user_name"":""Cool_User"",""title"":""Best Stream Ever"",""language"":""en"",""category_id"":""21779"",""category_name"":""Fortnite"",""is_mature"":false}}";
            using Stream requestBody = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(twitchNotification));
            var headers = new Dictionary<string, StringValues>
            {
                { TwitchConstants.EventSubHeaderNames.MessageId, new StringValues("e76c6bd4-55c9-4987-8304-da1588d8988b") },
                { TwitchConstants.EventSubHeaderNames.MessageRetry, new StringValues("0") },
                { TwitchConstants.EventSubHeaderNames.MessageSignature, new StringValues("sha256=144c8a2a7c859a77bb464e676571f7ffb634f4e34ac64a8794114596ee3eaeea") },
                { TwitchConstants.EventSubHeaderNames.MessageTimeStamp, new StringValues("2019-11-16T10:11:12.123Z") },
                { TwitchConstants.EventSubHeaderNames.MessageType, new StringValues(TwitchConstants.EventSubMessageTypes.Notification) },
                { TwitchConstants.EventSubHeaderNames.SubscriptionType, new StringValues(TwitchConstants.EventSubTypes.ChannelUpdate) },
                { TwitchConstants.EventSubHeaderNames.SubscriptionVersion, new StringValues("1") },
            };

            Mock<HttpContext> httpContextMoq = new Mock<HttpContext>();
            Mock<HttpResponse> httpResponse = new Mock<HttpResponse>();
            httpContextMoq.Setup(c => c.Request.Headers)
                .Returns(new HeaderDictionary(headers));
            httpContextMoq.Setup(c => c.Request.Body)
                .Returns(requestBody);
            httpContextMoq.Setup(c => c.Request.BodyReader)
                .Returns(PipeReader.Create(requestBody));
            httpContextMoq.Setup(c => c.Response)
                .Returns(httpResponse.Object);

            bool receivedEvent = false;

            Func<EventSubContext, TwitchEventSubChannelUpdateEvent, Task> assertingHandler = (context, twitchEvent) =>
            {
                Assert.NotNull(context);
                Assert.NotNull(twitchEvent);
                Assert.NotNull(context.Subscription);
                Assert.Equal(Guid.Parse("f1c2a387-161a-49f9-a165-0f21d7a4e1c4"), context.Subscription.Id);
                Assert.Equal(TwitchConstants.EventSubTypes.ChannelUpdate, context.Subscription.Type);
                Assert.Equal("1337", context.Subscription.Condition.BroadcasterUserId);

                Assert.Equal("1337", twitchEvent.BroadcasterUserId);
                Assert.Equal("cool_user", twitchEvent.BroadcasterUserLogin);
                Assert.Equal("Cool_User", twitchEvent.BroadcasterUserName);
                Assert.Equal("Best Stream Ever", twitchEvent.Title);
                Assert.Equal("en", twitchEvent.Language);
                Assert.Equal("21779", twitchEvent.CategoryId);
                Assert.Equal("Fortnite", twitchEvent.CategoryName);
                Assert.False(twitchEvent.IsMature);

                receivedEvent = true;
                return Task.CompletedTask;
            };

            var eventSubOptions = new EventSubOptions
            {
                WebHookSecret = "secret",
            };

            var mockLogger = new Mock<ILogger<EventSubHandler>>();
            var eventHandler = new HandlerRegistration<TwitchEventSubChannelUpdateEvent>(assertingHandler);

            var eventSubHandler = new EventSubHandler(
                new List<IHandlerRegistration> { eventHandler },
                new OptionsWrapper<EventSubOptions>(eventSubOptions),
                mockLogger.Object);

            await eventSubHandler.HandleRequestAsync(httpContextMoq.Object);

            Assert.True(receivedEvent);
        }

        [Fact]
        public async Task CanReceiveChannelPointCustomRewardRedemptionAddNotification()
        {
            const string twitchNotification = @"{""subscription"":{""id"":""f1c2a387-161a-49f9-a165-0f21d7a4e1c4"",""type"":""channel.channel_points_custom_reward_redemption.add"",""version"":""1"",""status"":""enabled"",""cost"":0,""condition"":{""broadcaster_user_id"":""1337""},""transport"":{""method"":""webhook"",""callback"":""https://example.com/webhooks/callback""},""created_at"":""2019-11-16T10:11:12.123Z""},""event"":{""id"":""1234"",""broadcaster_user_id"":""1337"",""broadcaster_user_login"":""cool_user"",""broadcaster_user_name"":""Cool_User"",""user_id"":""9001"",""user_login"":""cooler_user"",""user_name"":""Cooler_User"",""user_input"":""pogchamp"",""status"":""unfulfilled"",""reward"":{""id"":""9001"",""title"":""title"",""cost"":100,""prompt"":""reward prompt""},""redeemed_at"":""2020-07-15T17:16:03.17106713Z""}}";
            using Stream requestBody = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(twitchNotification));
            var headers = new Dictionary<string, StringValues>
            {
                { TwitchConstants.EventSubHeaderNames.MessageId, new StringValues("e76c6bd4-55c9-4987-8304-da1588d8988b") },
                { TwitchConstants.EventSubHeaderNames.MessageRetry, new StringValues("0") },
                { TwitchConstants.EventSubHeaderNames.MessageSignature, new StringValues("sha256=4c87325fea62c0082668ce6258de7eaccc0ee56e0d42242d54b60ce2f3ecb6b3") },
                { TwitchConstants.EventSubHeaderNames.MessageTimeStamp, new StringValues("2019-11-16T10:11:12.123Z") },
                { TwitchConstants.EventSubHeaderNames.MessageType, new StringValues(TwitchConstants.EventSubMessageTypes.Notification) },
                { TwitchConstants.EventSubHeaderNames.SubscriptionType, new StringValues(TwitchConstants.EventSubTypes.ChannelCustomRewardRedemptionAdd) },
                { TwitchConstants.EventSubHeaderNames.SubscriptionVersion, new StringValues("1") },
            };

            Mock<HttpContext> httpContextMoq = new Mock<HttpContext>();
            Mock<HttpResponse> httpResponse = new Mock<HttpResponse>();
            httpContextMoq.Setup(c => c.Request.Headers)
                .Returns(new HeaderDictionary(headers));
            httpContextMoq.Setup(c => c.Request.Body)
                .Returns(requestBody);
            httpContextMoq.Setup(c => c.Request.BodyReader)
                .Returns(PipeReader.Create(requestBody));
            httpContextMoq.Setup(c => c.Response)
                .Returns(httpResponse.Object);

            bool receivedEvent = false;

            Func<EventSubContext, TwitchEventSubChannelPointsCustomRewardRedemptionEvent, Task> assertingHandler = (context, twitchEvent) =>
            {
                Assert.NotNull(context);
                Assert.NotNull(twitchEvent);
                Assert.NotNull(context.Subscription);
                Assert.Equal(Guid.Parse("f1c2a387-161a-49f9-a165-0f21d7a4e1c4"), context.Subscription.Id);
                Assert.Equal(TwitchConstants.EventSubTypes.ChannelCustomRewardRedemptionAdd, context.Subscription.Type);

                Assert.Equal("1234", twitchEvent.Id);
                Assert.Equal("1337", twitchEvent.BroadcasterUserId);
                Assert.Equal("cool_user", twitchEvent.BroadcasterUserLogin);
                Assert.Equal("Cool_User", twitchEvent.BroadcasterUserName);
                Assert.Equal("9001", twitchEvent.UserId);
                Assert.Equal("cooler_user", twitchEvent.UserLogin);
                Assert.Equal("Cooler_User", twitchEvent.UserName);
                Assert.Equal("pogchamp", twitchEvent.UserInput);
                Assert.Equal("unfulfilled", twitchEvent.Status);
                Assert.Equal(DateTimeOffset.Parse("2020-07-15T17:16:03.17106713Z"), twitchEvent.RedeemedAt);

                Assert.NotNull(twitchEvent.Reward);
                Assert.Equal("9001", twitchEvent.Reward.Id);
                Assert.Equal("title", twitchEvent.Reward.Title);
                Assert.Equal("reward prompt", twitchEvent.Reward.Prompt);
                Assert.Equal(100, twitchEvent.Reward.Cost);

                receivedEvent = true;
                return Task.CompletedTask;
            };

            var eventSubOptions = new EventSubOptions
            {
                WebHookSecret = "secret",
            };

            var mockLogger = new Mock<ILogger<EventSubHandler>>();
            var eventHandler = new HandlerRegistration<TwitchEventSubChannelPointsCustomRewardRedemptionEvent>(assertingHandler);

            var eventSubHandler = new EventSubHandler(
                new List<IHandlerRegistration> { eventHandler },
                new OptionsWrapper<EventSubOptions>(eventSubOptions),
                mockLogger.Object);

            await eventSubHandler.HandleRequestAsync(httpContextMoq.Object);

            Assert.True(receivedEvent);
        }
    }
}
