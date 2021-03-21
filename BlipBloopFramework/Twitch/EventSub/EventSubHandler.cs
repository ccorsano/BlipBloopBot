using BlipBloopBot.Model.EventSub;
using BlipBloopBot.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
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
        private readonly EventSubOptions _options;
        private readonly ILogger _logger;

        public EventSubHandler(IOptions<EventSubOptions> options, ILogger<EventSubHandler> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task HandleRequestAsync(HttpContext context)
        {
            var eventSubContext = new EventSubContext();
            
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

        private TwitchEventSubCallbackPayload ReadPayload(string type, ReadOnlySequence<byte> data)
        {
            var reader = new Utf8JsonReader(data);

            return type switch
            {
                EventSubTypes.ChannelBan => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelBanEvent>>(ref reader),
                EventSubTypes.ChannelCheer => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelCheerEvent>>(ref reader),
                EventSubTypes.ChannelCustomRewardAdd => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelPointsCustomRewardDefinitionEvent>>(ref reader),
                EventSubTypes.ChannelCustomRewardUpdate => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelPointsCustomRewardDefinitionEvent>>(ref reader),
                EventSubTypes.ChannelCustomRewardRemove => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelPointsCustomRewardDefinitionEvent>>(ref reader),
                EventSubTypes.ChannelCustomRewardRedemptionAdd => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelPointsCustomRewardRedemptionEvent>>(ref reader),
                EventSubTypes.ChannelCustomRewardRedemptionUpdate => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelPointsCustomRewardRedemptionEvent>>(ref reader),
                EventSubTypes.ChannelCustomRewardRedemptionRemove => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelPointsCustomRewardRedemptionEvent>>(ref reader),
                EventSubTypes.ChannelFollow => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelFollowEvent>>(ref reader),
                EventSubTypes.ChannelModAdd => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelModAddEvent>>(ref reader),
                EventSubTypes.ChannelModRemove => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelModRemoveEvent>>(ref reader),
                EventSubTypes.ChannelRaid => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelRaidEvent>>(ref reader),
                EventSubTypes.ChannelSubscribe => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelSubscribeEvent>>(ref reader),
                EventSubTypes.ChannelUnban => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelUnbanEvent>>(ref reader),
                EventSubTypes.ChannelUpdate => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelUpdateEvent>>(ref reader),
                EventSubTypes.HypeTrainBegin => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubHypeTrainBeginEvent>>(ref reader),
                EventSubTypes.HypeTrainEnd => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubHypeTrainEndEvent>>(ref reader),
                EventSubTypes.HypeTrainProgress => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubHypeTrainProgressEvent>>(ref reader),
                EventSubTypes.StreamOffline => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubStreamOfflineEvent>>(ref reader),
                EventSubTypes.StreamOnline => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubStreamOnlineEvent>>(ref reader),
                EventSubTypes.UserRevoke => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubUserRevokeEvent>>(ref reader),
                EventSubTypes.UserUpdate => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubUserUpdateEvent>>(ref reader),
                _ => throw new NotImplementedException()
            };
        }

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

        private async Task<TwitchEventSubCallbackPayload<TEventSub>> ReadPayloadAsync<TEventSub>(Stream stream) where TEventSub : TwitchEventSubEvent
        {
            return await JsonSerializer.DeserializeAsync<TwitchEventSubCallbackPayload<TEventSub>>(stream);
        }
    }
}
