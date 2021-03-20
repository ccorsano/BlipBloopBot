using BlipBloopBot.Constants;
using BlipBloopBot.Model;
using BlipBloopBot.Model.EventSub;
using BlipBloopWeb.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static BlipBloopBot.Constants.TwitchConstants;

namespace BlipBloopWeb.Controllers
{
    [Route("twitch/eventsub")]
    public class TwitchEventSubController : Controller
    {
        private readonly EventSubOptions _options;
        private readonly ILogger _logger;

        public TwitchEventSubController(IOptions<EventSubOptions> options, ILogger<TwitchEventSubController> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Callback(CancellationToken cancellationToken)
        {
            foreach(var header in Request.Headers)
            {
                _logger.LogWarning("{headerName} = {headerValue}", header.Key, header.Value.FirstOrDefault());
            }

            string msgId = null;
            if (Request.Headers.TryGetValue(EventSubHeaderNames.MessageId, out var msgIdValues))
            {
                msgId = msgIdValues.First();
            }
            int retry;
            if (Request.Headers.TryGetValue(EventSubHeaderNames.MessageRetry, out var retryValues))
            {
                retry = int.Parse(retryValues.First());
            }
            string type = null;
            if (Request.Headers.TryGetValue(EventSubHeaderNames.MessageType, out var typeValues))
            {
                type = typeValues.First();
            }
            string signature = null;
            if (Request.Headers.TryGetValue(EventSubHeaderNames.MessageSignature, out var signatureValues))
            {
                signature = signatureValues.First();
            }
            string timestamp = null;
            if (Request.Headers.TryGetValue(EventSubHeaderNames.MessageTimeStamp, out var timestampValues))
            {
                timestamp = timestampValues.First();
            }
            string subType = null;
            if (Request.Headers.TryGetValue(EventSubHeaderNames.SubscriptionType, out var subTypeValues))
            {
                subType = subTypeValues.First();
            }
            string subVersion;
            if (Request.Headers.TryGetValue(EventSubHeaderNames.SubscriptionVersion, out var subVersionValues))
            {
                subVersion = subVersionValues.First();
            }

            var pool = ArrayPool<byte>.Shared;

            TwitchEventSubCallbackPayload payload = null;

            var key = System.Text.Encoding.UTF8.GetBytes(_options.WebHookSecret);
            using (var signatureAlg = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, key))
            {
                ReadResult result;
                signatureAlg.AppendData(System.Text.Encoding.UTF8.GetBytes(msgId));
                signatureAlg.AppendData(System.Text.Encoding.UTF8.GetBytes(timestamp));
                do
                {
                    result = await Request.BodyReader.ReadAsync(cancellationToken);
                    foreach (var segment in result.Buffer)
                    {
                        signatureAlg.AppendData(segment.Span);
                        _logger.LogWarning(System.Text.Encoding.UTF8.GetString(segment.Span));
                    }
                    payload = ReadPayload(subType, result.Buffer);
                } while (!result.IsCompleted && !result.IsCanceled);
                var hashBytes = signatureAlg.GetCurrentHash();
                var hashString = System.Convert.ToHexString(hashBytes).ToLowerInvariant();
                _logger.LogWarning("Signature = {signature}", hashString);
                if (hashString != signature.Split("=").Last())
                {
                    _logger.LogError("Signature mismatch {received} =/= {computer}", signature, hashString);
                    return new BadRequestResult();
                }

                if (type == EventSubMessageTypes.WebHookCallbackVerification)
                {
                    return new ContentResult
                    {
                        ContentType = "text/plain",
                        Content = payload.Challenge,
                        StatusCode = StatusCodes.Status200OK
                    };
                }

            }

            return new OkResult();
        }

        private TwitchEventSubCallbackPayload ReadPayload(string type, ReadOnlySequence<byte> data)
        {
            var reader = new Utf8JsonReader(data);
            
            return type switch
            {
                EventSubTypes.ChannelBan => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelBanEvent>>(ref reader),
                EventSubTypes.ChannelCheer => JsonSerializer.Deserialize< TwitchEventSubCallbackPayload<TwitchEventSubChannelCheerEvent>>(ref reader),
                EventSubTypes.ChannelCustomRewardAdd => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelPointsCustomRewardDefinitionEvent>>(ref reader),
                EventSubTypes.ChannelCustomRewardUpdate => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelPointsCustomRewardDefinitionEvent>>(ref reader),
                EventSubTypes.ChannelCustomRewardRemove => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelPointsCustomRewardDefinitionEvent>>(ref reader),
                EventSubTypes.ChannelCustomRewardRedemptionAdd => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelPointsCustomRewardRedemptionEvent>>(ref reader),
                EventSubTypes.ChannelCustomRewardRedemptionUpdate => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelPointsCustomRewardRedemptionEvent>>(ref reader),
                EventSubTypes.ChannelCustomRewardRedemptionRemove => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelPointsCustomRewardRedemptionEvent>>(ref reader),
                EventSubTypes.ChannelFollow => JsonSerializer.Deserialize<TwitchEventSubCallbackPayload<TwitchEventSubChannelFollowEvent>>(ref reader),
                EventSubTypes.ChannelModAdd => JsonSerializer.Deserialize< TwitchEventSubCallbackPayload<TwitchEventSubChannelModAddEvent>>(ref reader),
                EventSubTypes.ChannelModRemove => JsonSerializer.Deserialize< TwitchEventSubCallbackPayload<TwitchEventSubChannelModRemoveEvent>>(ref reader),
                EventSubTypes.ChannelRaid => JsonSerializer.Deserialize< TwitchEventSubCallbackPayload<TwitchEventSubChannelRaidEvent>>(ref reader),
                EventSubTypes.ChannelSubscribe => JsonSerializer.Deserialize< TwitchEventSubCallbackPayload<TwitchEventSubChannelSubscribeEvent>>(ref reader),
                EventSubTypes.ChannelUnban => JsonSerializer.Deserialize< TwitchEventSubCallbackPayload<TwitchEventSubChannelUnbanEvent>>(ref reader),
                EventSubTypes.ChannelUpdate => JsonSerializer.Deserialize< TwitchEventSubCallbackPayload<TwitchEventSubChannelUpdateEvent>>(ref reader),
                EventSubTypes.HypeTrainBegin => JsonSerializer.Deserialize< TwitchEventSubCallbackPayload<TwitchEventSubHypeTrainBeginEvent>>(ref reader),
                EventSubTypes.HypeTrainEnd => JsonSerializer.Deserialize< TwitchEventSubCallbackPayload<TwitchEventSubHypeTrainEndEvent>>(ref reader),
                EventSubTypes.HypeTrainProgress => JsonSerializer.Deserialize< TwitchEventSubCallbackPayload<TwitchEventSubHypeTrainProgressEvent>>(ref reader),
                EventSubTypes.StreamOffline => JsonSerializer.Deserialize< TwitchEventSubCallbackPayload<TwitchEventSubStreamOfflineEvent>>(ref reader),
                EventSubTypes.StreamOnline => JsonSerializer.Deserialize< TwitchEventSubCallbackPayload<TwitchEventSubStreamOnlineEvent>>(ref reader),
                EventSubTypes.UserRevoke => JsonSerializer.Deserialize< TwitchEventSubCallbackPayload<TwitchEventSubUserRevokeEvent>>(ref reader),
                EventSubTypes.UserUpdate => JsonSerializer.Deserialize< TwitchEventSubCallbackPayload<TwitchEventSubUserUpdateEvent>>(ref reader),
                _ => throw new NotImplementedException()
            };
        }
    }
}
