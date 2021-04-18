using Conceptoire.Twitch.Authentication;
using Conceptoire.Twitch.Constants;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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
        private TaskCompletionSource _connection;

        private ConcurrentDictionary<string, TaskCompletionSource> _listenRequests = new ConcurrentDictionary<string, TaskCompletionSource>();

        private readonly ReadOnlyMemory<byte> PingPayload = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("{\"Type\":\"PING\"}"));


        public TwitchPubSubClient(IBotAuthenticated twitchAuthenticated, ILogger<TwitchPubSubClient> logger)
        {
            _authenticated = twitchAuthenticated;
            _logger = logger;
            _webSocket = new ClientWebSocket();
            _inBuffer = new byte[65536]; // Probably overkill ...
            _connection = new TaskCompletionSource();
        }

        public void Dispose()
        {
            _webSocket.Dispose();
        }

        public Task Listen(Topic topic, CancellationToken cancellationToken)
            => Listen(new Topic[] { topic }, cancellationToken);

        public async Task Listen(Topic[] topics, CancellationToken cancellationToken)
        {
            await _connection.Task;

            var request = new TwitchPubSubRequest
            {
                Data = new TwitchPubSubRequestData
                {
                    AuthToken = _authenticated.Token,
                    Topics = topics.Select(t => t.ToString()).ToArray()
                }
            };
            var message = JsonSerializer.SerializeToUtf8Bytes(request);
            var listenCompletionSource = new TaskCompletionSource();
            // Add the request to the pending listen requests, waiting for an incoming RESPONSE message
            if (!_listenRequests.TryAdd(request.Nonce, listenCompletionSource))
            {
                throw new Exception("Nonce collision ? not supposed to happen, abort mission");
            }
            await _webSocket.SendAsync(message, WebSocketMessageType.Text, true, cancellationToken);
            await listenCompletionSource.Task;
            // TODO: timeout
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _wsCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var wsCancellation = _wsCancellationTokenSource.Token;
            await _webSocket.ConnectAsync(new Uri(TwitchConstants.PubSubWebSocketUri), cancellationToken);
            _lastPingSent = DateTimeOffset.UtcNow;
            _connection.SetResult();
            _wsTask = Task.Run(async () =>
            {
                try
                {
                    while (!_wsCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(wsCancellation);
                        timeoutCancellation.CancelAfter(TimeSpan.FromMinutes(5) - (DateTimeOffset.UtcNow - _lastPingSent));

                        var receiveInfo = await _webSocket.ReceiveAsync(_inBuffer, timeoutCancellation.Token);
                        if (receiveInfo.MessageType == WebSocketMessageType.Binary)
                        {
                            throw new FormatException("Twitch PubSub is expected to communicate using Text WS messages only.");
                        }

                        try
                        {
                            _logger.LogInformation(Encoding.UTF8.GetString(_inBuffer.AsSpan(0, receiveInfo.Count)));
                            var receivedMessage  = JsonSerializer.Deserialize<ServerMessage>(_inBuffer.AsSpan(0, receiveInfo.Count));

                            if (receivedMessage.Type == TwitchConstants.PUBSUB_SERVER_RESPONSE)
                            {
                                if (!_listenRequests.TryRemove(receivedMessage.Nonce, out TaskCompletionSource listenCompletion))
                                {
                                    _logger.LogError("Received a RESPONSE from an unknown Nonce {nonce} ({error})", receivedMessage.Nonce, receivedMessage.Error ?? "Success");
                                    // TODO: send UNLISTEN
                                }
                                if (string.IsNullOrEmpty(receivedMessage.Error))
                                {
                                    _logger.LogInformation("LISTEN accepted for nonce {nonce}", receivedMessage.Nonce);
                                    listenCompletion.SetResult();
                                }
                                else
                                {
                                    _logger.LogError("LISTEN for Nonce {nonce} failed with error {error}", receivedMessage.Nonce, receivedMessage.Error ?? "Success");
                                    listenCompletion.SetException(new Exception($"Received error from twitch PubSub server {receivedMessage.Error}"));
                                }
                            }
                            else if (receivedMessage.Type == TwitchConstants.PUBSUB_SERVER_MESSAGE)
                            {
                                _logger.LogInformation("PubSub > {topic} : Message received", receivedMessage.Data.Topic);
                            }
                        } catch(JsonException jsonException)
                        {
                            _logger.LogError(jsonException, "Error parsing PubSub message {message}", Encoding.UTF8.GetString(_inBuffer.AsSpan(0, receiveInfo.Count)));
                        }
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Critical error on PubSub listener");
                    throw;
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
