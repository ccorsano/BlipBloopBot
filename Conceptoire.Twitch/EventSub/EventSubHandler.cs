using Conceptoire.Twitch.Model.EventSub;
using Conceptoire.Twitch.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using static Conceptoire.Twitch.Constants.TwitchConstants;

namespace Conceptoire.Twitch.EventSub
{
    public class EventSubHandler
    {
        private readonly IEnumerable<IHandlerRegistration> _handlers;
        private readonly EventSubOptions _options;
        private readonly ILogger _logger;

        public EventSubHandler(IEnumerable<IHandlerRegistration> handlers, IOptions<EventSubOptions> options, ILogger<EventSubHandler> logger)
        {
            _handlers = handlers;
            _options = options.Value;

            if (string.IsNullOrEmpty(_options.WebHookSecret))
            {
                throw new ArgumentException("Missing WebHookSecret");
            }
            _logger = logger;
        }

        public async Task HandleRequestAsync(HttpContext context)
        {
            var eventSubContext = new EventSubContext
            {
                Logger = _logger,
                Services = context.RequestServices,
            };

            if (!ParseHeaders(context.Request, eventSubContext))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            TwitchEventSubCallbackPayload payload = null;
            var isFirstSegment = true;

            var key = System.Text.Encoding.UTF8.GetBytes(_options.WebHookSecret);
            using (var signatureAlg = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, key))
            {
                ReadResult result;

                signatureAlg.AppendData(System.Text.Encoding.UTF8.GetBytes(eventSubContext.Headers.MessageId));
                signatureAlg.AppendData(System.Text.Encoding.UTF8.GetBytes(eventSubContext.Headers.MessageTimeStamp));

                do
                {
                    result = await context.Request.BodyReader.ReadAsync();

                    foreach (var segment in result.Buffer)
                    {
                        signatureAlg.AppendData(segment.Span);
                        _logger.LogWarning(System.Text.Encoding.UTF8.GetString(segment.Span));
                    }

                    // If the first segment contains the whole body, read the payload from the segment
                    if (isFirstSegment && result.Buffer.IsSingleSegment)
                    {
                        payload = ReadPayload(eventSubContext.Headers.SubscriptionType, result.Buffer);
                        break;
                    }
                    isFirstSegment = false;
                } while (!result.IsCompleted && !result.IsCanceled);

                // Finalizing signature validation
                var hashBytes = signatureAlg.GetCurrentHash();
                var hashString = Convert.ToHexString(hashBytes).ToLowerInvariant();
                _logger.LogWarning("Signature = {signature}", hashString);
                if (hashString != eventSubContext.Headers.MessageSignature.Split("=").Last())
                {
                    _logger.LogError("Signature mismatch {received} =/= {computer}", eventSubContext.Headers.MessageSignature, hashString);

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                // Degraded case: Body was more than a single segment, use Stream interface
                if (payload == null)
                {
                    payload = await ReadPayloadAsync(eventSubContext.Headers.SubscriptionType, context.Request.Body);
                }

                if (eventSubContext.Headers.MessageType == EventSubMessageTypes.WebHookCallbackVerification)
                {
                    await context.Response.WriteAsync(payload.Challenge);
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    context.Response.ContentType = "text/plain";
                    return;
                }

                eventSubContext.Subscription = payload.Subscription;

                foreach (var handler in _handlers)
                {
                    if (handler.CanHandleEvent(eventSubContext, payload.BaseEvent))
                    {
                        await handler.OnEventSubNotification(eventSubContext, payload.BaseEvent);
                    }
                }
            }

            context.Response.StatusCode = StatusCodes.Status200OK;
        }

        public bool ParseHeaders(HttpRequest request, EventSubContext context)
        {
            if (!request.Headers.TryGetValue(EventSubHeaderNames.MessageId, out var msgIdValues))
            {
                return false;
            }
            if (!request.Headers.TryGetValue(EventSubHeaderNames.MessageRetry, out var retryValues))
            {
                return false;
            }
            if (!request.Headers.TryGetValue(EventSubHeaderNames.MessageType, out var typeValues))
            {
                return false;
            }
            if (!request.Headers.TryGetValue(EventSubHeaderNames.MessageSignature, out var signatureValues))
            {
                return false;
            }
            if (!request.Headers.TryGetValue(EventSubHeaderNames.MessageTimeStamp, out var timestampValues))
            {
                return false;
            }
            if (!request.Headers.TryGetValue(EventSubHeaderNames.SubscriptionType, out var subTypeValues))
            {
                return false;
            }
            if (!request.Headers.TryGetValue(EventSubHeaderNames.SubscriptionVersion, out var subVersionValues))
            {
                return false;
            }

            context.Headers.MessageId = msgIdValues.First();
            context.Headers.MessageRetry = int.Parse(retryValues.First());
            context.Headers.MessageType = typeValues.First();
            context.Headers.MessageSignature = signatureValues.First();
            context.Headers.MessageTimeStamp = timestampValues.First();
            context.Headers.SubscriptionType = subTypeValues.First();
            context.Headers.SubscriptionVersion = subVersionValues.First();

            return true;
        }

        private TwitchEventSubCallbackPayload ReadPayload(string type, ReadOnlySequence<byte> data) => type switch
        {
            EventSubTypes.ChannelBan => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelBanEvent),
            EventSubTypes.ChannelCheer => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelCheerEvent),
            EventSubTypes.ChannelCustomRewardAdd => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelPointsCustomRewardDefinitionEvent),
            EventSubTypes.ChannelCustomRewardUpdate => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelPointsCustomRewardDefinitionEvent),
            EventSubTypes.ChannelCustomRewardRemove => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelPointsCustomRewardDefinitionEvent),
            EventSubTypes.ChannelCustomRewardRedemptionAdd => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelPointsCustomRewardRedemptionEvent),
            EventSubTypes.ChannelCustomRewardRedemptionUpdate => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelPointsCustomRewardRedemptionEvent),
            EventSubTypes.ChannelCustomRewardRedemptionRemove => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelPointsCustomRewardRedemptionEvent),
            EventSubTypes.ChannelFollow => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelFollowEvent),
            EventSubTypes.ChannelModAdd => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelModAddEvent),
            EventSubTypes.ChannelModRemove => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelModRemoveEvent),
            EventSubTypes.ChannelRaid => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelRaidEvent),
            EventSubTypes.ChannelSubscribe => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelSubscribeEvent),
            EventSubTypes.ChannelUnban => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelUnbanEvent),
            EventSubTypes.ChannelUpdate => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelUpdateEvent),
            EventSubTypes.HypeTrainBegin => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubHypeTrainBeginEvent),
            EventSubTypes.HypeTrainEnd => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubHypeTrainEndEvent),
            EventSubTypes.HypeTrainProgress => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubHypeTrainProgressEvent),
            EventSubTypes.StreamOffline => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubStreamOfflineEvent),
            EventSubTypes.StreamOnline => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubStreamOnlineEvent),
            EventSubTypes.UserRevoke => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubUserRevokeEvent),
            EventSubTypes.UserUpdate => ReadPayloadAsync(data, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubUserUpdateEvent),
            _ => throw new NotImplementedException()
        };

        private async Task<TwitchEventSubCallbackPayload> ReadPayloadAsync(string type, Stream stream) => type switch
        {
            EventSubTypes.ChannelBan => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelBanEvent),
            EventSubTypes.ChannelCheer => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelCheerEvent),
            EventSubTypes.ChannelCustomRewardAdd => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelPointsCustomRewardDefinitionEvent),
            EventSubTypes.ChannelCustomRewardUpdate => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelPointsCustomRewardDefinitionEvent),
            EventSubTypes.ChannelCustomRewardRemove => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelPointsCustomRewardDefinitionEvent),
            EventSubTypes.ChannelCustomRewardRedemptionAdd => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelPointsCustomRewardRedemptionEvent),
            EventSubTypes.ChannelCustomRewardRedemptionUpdate => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelPointsCustomRewardRedemptionEvent),
            EventSubTypes.ChannelCustomRewardRedemptionRemove => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelPointsCustomRewardRedemptionEvent),
            EventSubTypes.ChannelFollow => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelFollowEvent),
            EventSubTypes.ChannelModAdd => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelModAddEvent),
            EventSubTypes.ChannelModRemove => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelModRemoveEvent),
            EventSubTypes.ChannelRaid => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelRaidEvent),
            EventSubTypes.ChannelSubscribe => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelSubscribeEvent),
            EventSubTypes.ChannelUnban => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelUnbanEvent),
            EventSubTypes.ChannelUpdate => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubChannelUpdateEvent),
            EventSubTypes.HypeTrainBegin => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubHypeTrainBeginEvent),
            EventSubTypes.HypeTrainEnd => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubHypeTrainEndEvent),
            EventSubTypes.HypeTrainProgress => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubHypeTrainProgressEvent),
            EventSubTypes.StreamOffline => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubStreamOfflineEvent),
            EventSubTypes.StreamOnline => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubStreamOnlineEvent),
            EventSubTypes.UserRevoke => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubUserRevokeEvent),
            EventSubTypes.UserUpdate => await ReadPayloadAsync(stream, TwitchEventSubJsonContext.Default.TwitchEventSubCallbackPayloadTwitchEventSubUserUpdateEvent),
            _ => throw new NotImplementedException()
        };

        private TwitchEventSubCallbackPayload<TEventSub> ReadPayloadAsync<TEventSub>(ReadOnlySequence<byte> data, JsonTypeInfo<TwitchEventSubCallbackPayload<TEventSub>> typeInfo) where TEventSub : TwitchEventSubEvent
        {
            var payload = JsonSerializer.Deserialize(data.FirstSpan, typeInfo);
            payload.BaseEvent = payload.Event;
            return payload;
        }

        private async Task<TwitchEventSubCallbackPayload<TEventSub>> ReadPayloadAsync<TEventSub>(Stream stream, JsonTypeInfo<TwitchEventSubCallbackPayload<TEventSub>> typeInfo) where TEventSub : TwitchEventSubEvent
        {
            var payload = await JsonSerializer.DeserializeAsync(stream, typeInfo);
            payload.BaseEvent = payload.Event;
            return payload;
        }
    }
}
