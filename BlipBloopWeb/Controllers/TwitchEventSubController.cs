using BlipBloopBot.Constants;
using BlipBloopBot.Model;
using BlipBloopBot.Model.EventSub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlipBloopWeb.Controllers
{
    [Route("twitch/eventsub")]
    public class TwitchEventSubController : Controller
    {
        private readonly ILogger _logger;

        public TwitchEventSubController(ILogger<TwitchEventSubController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Callback(CancellationToken cancellationToken)
        {
            var secret = "7517d8c3-28f4-4372-947a-0069efa2dcf6";

            foreach(var header in Request.Headers)
            {
                _logger.LogWarning("{headerName} = {headerValue}", header.Key, header.Value.FirstOrDefault());
            }

            string msgId = null;
            if (Request.Headers.TryGetValue(TwitchConstants.EVENTSUB_HEADERNAME_MSGID, out var msgIdValues))
            {
                msgId = msgIdValues.First();
            }
            int retry;
            if (Request.Headers.TryGetValue(TwitchConstants.EVENTSUB_HEADERNAME_RETRY, out var retryValues))
            {
                retry = int.Parse(retryValues.First());
            }
            string type;
            if (Request.Headers.TryGetValue(TwitchConstants.EVENTSUB_HEADERNAME_MSGTYPE, out var typeValues))
            {
                type = typeValues.First();
            }
            string signature = null;
            if (Request.Headers.TryGetValue(TwitchConstants.EVENTSUB_HEADERNAME_SIGNATURE, out var signatureValues))
            {
                signature = signatureValues.First();
            }
            string timestamp = null;
            if (Request.Headers.TryGetValue(TwitchConstants.EVENTSUB_HEADERNAME_TIMESTAMP, out var timestampValues))
            {
                timestamp = timestampValues.First();
            }
            string subType;
            if (Request.Headers.TryGetValue(TwitchConstants.EVENTSUB_HEADERNAME_SUBTYPE, out var subTypeValues))
            {
                subType = subTypeValues.First();
            }
            string subVersion;
            if (Request.Headers.TryGetValue(TwitchConstants.EVENTSUB_HEADERNAME_SUBVERSION, out var subVersionValues))
            {
                subVersion = subVersionValues.First();
            }

            var pool = ArrayPool<byte>.Shared;

            var key = System.Text.Encoding.UTF8.GetBytes(secret);
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
                } while (!result.IsCompleted && !result.IsCanceled);
                var hashBytes = signatureAlg.GetCurrentHash();
                var hashString = System.Convert.ToHexString(hashBytes).ToLowerInvariant();
                _logger.LogWarning("Signature = {signature}", hashString);
                if (hashString != signature.Split("=").Last())
                {
                    _logger.LogError("Signature mismatch {received} =/= {computer}", signature, hashString);
                }
            }

            return new OkResult();
        }
    }
}
