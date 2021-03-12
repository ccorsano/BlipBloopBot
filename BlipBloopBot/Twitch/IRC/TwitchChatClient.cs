using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch.IRC
{
    /// <summary>
    /// A small Twitch IRC over WebSocket client.
    /// Aimed to be low-allocation, low-overhead and extensible.
    /// </summary>
    public class TwitchChatClient : IDisposable
    {
        private readonly TwitchChatClientOptions _options;
        private readonly ClientWebSocket _webSocket;
        private readonly byte[] _outBuffer;
        private readonly byte[] _inBuffer;
        private readonly List<IMessageProcessor> _processors;
        private readonly ILogger _logger;

        public TwitchChatClient(IEnumerable<IMessageProcessor> processors, IOptions<TwitchChatClientOptions> options, ILogger<TwitchChatClient> logger)
        {
            _options = options.Value;
            _webSocket = new ClientWebSocket();
            _inBuffer = new byte[1024];
            _outBuffer = new byte[1024];
            _processors = processors.ToList();
            _logger = logger;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            var wsUri = new Uri(_options.Endpoint);
            await _webSocket.ConnectAsync(wsUri, cancellationToken);

            await SendCommandAsync("PASS", $"oauth:{_options.OAuthToken}", cancellationToken);
            await SendCommandAsync("NICK", _options.UserName, cancellationToken);
            await ReceiveIRCMessage(cancellationToken);
        }

        public async Task SendCommandAsync(string cmd, string message, CancellationToken cancellationToken)
        {
            var sendText = $"{cmd} {message}";
            var length = Encoding.UTF8.GetBytes(sendText.AsSpan(), _outBuffer.AsSpan());

            await _webSocket.SendAsync(_outBuffer.AsMemory().Slice(0, length), WebSocketMessageType.Text, true, cancellationToken);
        }

        public async Task ReceiveIRCMessage(CancellationToken cancellationToken)
        {
            var rcvResult = await _webSocket.ReceiveAsync(_inBuffer, cancellationToken);
            var stringMessage = Encoding.UTF8.GetString(_inBuffer.AsSpan().Slice(0, rcvResult.Count));

            ParseMessage(stringMessage);
        }

        private void ParseMessage(string receivedMessage)
        {
            foreach (var line in receivedMessage.SplitLines())
            {
                foreach(var processor in _processors)
                {
                    processor.OnMessage(line.Message);
                }
            }
        }

        public void Dispose()
        {
            _webSocket.Dispose();
        }
    }
}
