using BlipBloopBot.Model.EventSub;
using BlipBloopBot.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using static BlipBloopBot.Constants.TwitchConstants;

namespace BlipBloopBot.Twitch.EventSub
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
            };
            
            if (! ParseHeaders(context.Request, eventSubContext))
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

                foreach(var handler in _handlers)
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
            if (! request.Headers.TryGetValue(EventSubHeaderNames.MessageId, out var msgIdValues))
            {
                return false;
            }
            if (! request.Headers.TryGetValue(EventSubHeaderNames.MessageRetry, out var retryValues))
            {
                return false;
            }
            if (! request.Headers.TryGetValue(EventSubHeaderNames.MessageType, out var typeValues))
            {
                return false;
            }
            if (! request.Headers.TryGetValue(EventSubHeaderNames.MessageSignature, out var signatureValues))
            {
                return false;
            }
            if (! request.Headers.TryGetValue(EventSubHeaderNames.MessageTimeStamp, out var timestampValues))
            {
                return false;
            }
            if (! request.Headers.TryGetValue(EventSubHeaderNames.SubscriptionType, out var subTypeValues))
            {
                return false;
            }
            if (! request.Headers.TryGetValue(EventSubHeaderNames.SubscriptionVersion, out var subVersionValues))
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
            EventSubTypes.ChannelBan => ReadPayloadAsync<TwitchEventSubChannelBanEvent>(data),
            EventSubTypes.ChannelCheer => ReadPayloadAsync<TwitchEventSubChannelCheerEvent>(data),
            EventSubTypes.ChannelCustomRewardAdd => ReadPayloadAsync<TwitchEventSubChannelPointsCustomRewardDefinitionEvent>(data),
            EventSubTypes.ChannelCustomRewardUpdate => ReadPayloadAsync<TwitchEventSubChannelPointsCustomRewardDefinitionEvent>(data),
            EventSubTypes.ChannelCustomRewardRemove => ReadPayloadAsync<TwitchEventSubChannelPointsCustomRewardDefinitionEvent>(data),
            EventSubTypes.ChannelCustomRewardRedemptionAdd => ReadPayloadAsync<TwitchEventSubChannelPointsCustomRewardRedemptionEvent>(data),
            EventSubTypes.ChannelCustomRewardRedemptionUpdate => ReadPayloadAsync<TwitchEventSubChannelPointsCustomRewardRedemptionEvent>(data),
            EventSubTypes.ChannelCustomRewardRedemptionRemove => ReadPayloadAsync<TwitchEventSubChannelPointsCustomRewardRedemptionEvent>(data),
            EventSubTypes.ChannelFollow => ReadPayloadAsync<TwitchEventSubChannelFollowEvent>(data),
            EventSubTypes.ChannelModAdd => ReadPayloadAsync<TwitchEventSubChannelModAddEvent>(data),
            EventSubTypes.ChannelModRemove => ReadPayloadAsync<TwitchEventSubChannelModRemoveEvent>(data),
            EventSubTypes.ChannelRaid => ReadPayloadAsync<TwitchEventSubChannelRaidEvent>(data),
            EventSubTypes.ChannelSubscribe => ReadPayloadAsync<TwitchEventSubChannelSubscribeEvent>(data),
            EventSubTypes.ChannelUnban => ReadPayloadAsync<TwitchEventSubChannelUnbanEvent>(data),
            EventSubTypes.ChannelUpdate => ReadPayloadAsync<TwitchEventSubChannelUpdateEvent>(data),
            EventSubTypes.HypeTrainBegin => ReadPayloadAsync<TwitchEventSubHypeTrainBeginEvent>(data),
            EventSubTypes.HypeTrainEnd => ReadPayloadAsync<TwitchEventSubHypeTrainEndEvent>(data),
            EventSubTypes.HypeTrainProgress => ReadPayloadAsync<TwitchEventSubHypeTrainProgressEvent>(data),
            EventSubTypes.StreamOffline => ReadPayloadAsync<TwitchEventSubStreamOfflineEvent>(data),
            EventSubTypes.StreamOnline => ReadPayloadAsync<TwitchEventSubStreamOnlineEvent>(data),
            EventSubTypes.UserRevoke => ReadPayloadAsync<TwitchEventSubUserRevokeEvent>(data),
            EventSubTypes.UserUpdate => ReadPayloadAsync<TwitchEventSubUserUpdateEvent>(data),
            _ => throw new NotImplementedException()
        };

        private async Task<TwitchEventSubCallbackPayload> ReadPayloadAsync(string type, Stream stream) => type switch
        {
            EventSubTypes.ChannelBan => await ReadPayloadAsync<TwitchEventSubChannelBanEvent>(stream),
            EventSubTypes.ChannelCheer => await ReadPayloadAsync<TwitchEventSubChannelCheerEvent>(stream),
            EventSubTypes.ChannelCustomRewardAdd => await ReadPayloadAsync<TwitchEventSubChannelPointsCustomRewardDefinitionEvent>(stream),
            EventSubTypes.ChannelCustomRewardUpdate => await ReadPayloadAsync<TwitchEventSubChannelPointsCustomRewardDefinitionEvent>(stream),
            EventSubTypes.ChannelCustomRewardRemove => await ReadPayloadAsync<TwitchEventSubChannelPointsCustomRewardDefinitionEvent>(stream),
            EventSubTypes.ChannelCustomRewardRedemptionAdd => await ReadPayloadAsync<TwitchEventSubChannelPointsCustomRewardRedemptionEvent>(stream),
            EventSubTypes.ChannelCustomRewardRedemptionUpdate => await ReadPayloadAsync<TwitchEventSubChannelPointsCustomRewardRedemptionEvent>(stream),
            EventSubTypes.ChannelCustomRewardRedemptionRemove => await ReadPayloadAsync<TwitchEventSubChannelPointsCustomRewardRedemptionEvent>(stream),
            EventSubTypes.ChannelFollow => await ReadPayloadAsync<TwitchEventSubChannelFollowEvent>(stream),
            EventSubTypes.ChannelModAdd => await ReadPayloadAsync<TwitchEventSubChannelModAddEvent>(stream),
            EventSubTypes.ChannelModRemove => await ReadPayloadAsync<TwitchEventSubChannelModRemoveEvent>(stream),
            EventSubTypes.ChannelRaid => await ReadPayloadAsync<TwitchEventSubChannelRaidEvent>(stream),
            EventSubTypes.ChannelSubscribe => await ReadPayloadAsync<TwitchEventSubChannelSubscribeEvent>(stream),
            EventSubTypes.ChannelUnban => await ReadPayloadAsync<TwitchEventSubChannelUnbanEvent>(stream),
            EventSubTypes.ChannelUpdate => await ReadPayloadAsync<TwitchEventSubChannelUpdateEvent>(stream),
            EventSubTypes.HypeTrainBegin => await ReadPayloadAsync<TwitchEventSubHypeTrainBeginEvent>(stream),
            EventSubTypes.HypeTrainEnd => await ReadPayloadAsync<TwitchEventSubHypeTrainEndEvent>(stream),
            EventSubTypes.HypeTrainProgress => await ReadPayloadAsync<TwitchEventSubHypeTrainProgressEvent>(stream),
            EventSubTypes.StreamOffline => await ReadPayloadAsync<TwitchEventSubStreamOfflineEvent>(stream),
            EventSubTypes.StreamOnline => await ReadPayloadAsync<TwitchEventSubStreamOnlineEvent>(stream),
            EventSubTypes.UserRevoke => await ReadPayloadAsync<TwitchEventSubUserRevokeEvent>(stream),
            EventSubTypes.UserUpdate => await ReadPayloadAsync<TwitchEventSubUserUpdateEvent>(stream),
            _ => throw new NotImplementedException()
        };

        private TwitchEventSubCallbackPayload<TEventSub> ReadPayloadAsync<TEventSub>(ReadOnlySequence<byte> data) where TEventSub : TwitchEventSubEvent
        {
            var payload = JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TEventSub>>(data.FirstSpan);
            payload.BaseEvent = payload.Event;
            return payload;
        }

        private async Task<TwitchEventSubCallbackPayload<TEventSub>> ReadPayloadAsync<TEventSub>(Stream stream) where TEventSub : TwitchEventSubEvent
        {
            var payload = await JsonSerializer.DeserializeAsync<TwitchEventSubCallbackPayload<TEventSub>>(stream);
            payload.BaseEvent = payload.Event;
            return payload;
        }
    }
}
