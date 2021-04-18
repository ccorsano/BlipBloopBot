using Conceptoire.Twitch.Authentication;
using Conceptoire.Twitch.Constants;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.PubSub
{
    public class TwitchPubSubClient : IHostedService, IDisposable
    {
        private readonly IAuthenticated _authenticated;
        private readonly ILogger<TwitchPubSubClient> _logger;
        private readonly byte[] _inBuffer;

        private CancellationTokenSource _wsCancellationTokenSource;
        private ClientWebSocket _webSocket;
        private Task _wsTask;
        private Task _wsKeepAliveTask;
        private DateTimeOffset _lastPingSent;

        private readonly ReadOnlyMemory<byte> PingPayload = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("{\"Type\":\"PING\"}"));


        public TwitchPubSubClient(IAuthenticated twitchAuthenticated, ILogger<TwitchPubSubClient> logger)
        {
            _authenticated = twitchAuthenticated;
            _logger = logger;
            _webSocket = new ClientWebSocket();
            _inBuffer = new byte[65536]; // Probably overkill ...
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
        }

        public void Dispose()
        {
            _webSocket.Dispose();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _wsCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var wsCancellation = _wsCancellationTokenSource.Token;
            await _webSocket.ConnectAsync(new Uri(TwitchConstants.PubSubWebSocketUri), cancellationToken);
            _wsTask = Task.Run(async () =>
            {
                while(! _wsCancellationTokenSource.Token.IsCancellationRequested)
                {
                    var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(wsCancellation);
                    timeoutCancellation.CancelAfter(TimeSpan.FromMinutes(5) - (DateTimeOffset.UtcNow - _lastPingSent));

                    var receiveInfo = await _webSocket.ReceiveAsync(_inBuffer, timeoutCancellation.Token);
                    if (receiveInfo.MessageType == WebSocketMessageType.Binary)
                    {
                        throw new FormatException("Twitch PubSub is expected to communicate using Text WS messages only.");
                    }
                    var result = JsonSerializer.Deserialize<TwitchPubSubResult>(_inBuffer.AsSpan(0, receiveInfo.Count));

                }
            });
            _wsKeepAliveTask = Task.Run(async () =>
            {
                while (!_wsCancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMinutes(4));
                    await _webSocket.SendAsync(PingPayload, WebSocketMessageType.Text, true, wsCancellation);
                    _lastPingSent = DateTimeOffset.UtcNow;
                }
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _wsCancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}
