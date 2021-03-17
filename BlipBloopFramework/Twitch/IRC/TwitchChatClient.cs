using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
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
        private string _joinedChannel;
        private bool _receivedPing;

        private ConcurrentQueue<string> _outMessageQueue = new ConcurrentQueue<string>();

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
            await ReceiveIRCMessage(new List<(string, IMessageProcessor)>(), cancellationToken);
        }

        public async Task JoinAsync(string channelName, CancellationToken cancellationToken)
        {
            var botUserName = _options.UserName.ToLowerInvariant();
            await SendCommandAsync("JOIN", $"#{channelName}", cancellationToken);
            await SendCommandAsync($":{botUserName}!{botUserName}@{botUserName}.tmi.twitch.tv JOIN", $"#{channelName}", cancellationToken);
            _joinedChannel = channelName;
        }

        public async Task SendCommandAsync(string cmd, string message, CancellationToken cancellationToken)
        {
            var sendText = $"{cmd} {message}";
            var length = Encoding.UTF8.GetBytes(sendText.AsSpan(), _outBuffer.AsSpan());

            await _webSocket.SendAsync(_outBuffer.AsMemory().Slice(0, length), WebSocketMessageType.Text, true, cancellationToken);
        }

        public async Task SendMessageAsync(string message, CancellationToken cancellationToken)
        {
            if (_joinedChannel == null)
            {
                throw new InvalidOperationException("No active channel");
            }

            var sendText = $"PRIVMSG #{_joinedChannel} :{message}";
            var length = Encoding.UTF8.GetBytes(sendText.AsSpan(), _outBuffer.AsSpan());

            await _webSocket.SendAsync(_outBuffer.AsMemory().Slice(0, length), WebSocketMessageType.Text, true, cancellationToken);
        }

        public async Task ReceiveIRCMessage(IEnumerable<(string,IMessageProcessor)> processors, CancellationToken cancellationToken)
        {
            var rcvResult = await _webSocket.ReceiveAsync(_inBuffer, cancellationToken);
            var stringMessage = Encoding.UTF8.GetString(_inBuffer.AsSpan().Slice(0, rcvResult.Count));

            ParseMessage(stringMessage, processors);

            if (_receivedPing)
            {
                await SendCommandAsync("PONG", ":tmi.twitch.tv", cancellationToken);
                _receivedPing = false;
            }
            
            while (_outMessageQueue.TryDequeue(out var msg))
            {
                await SendMessageAsync(msg, cancellationToken);
            }
        }

        private void ParseMessage(string receivedMessage, IEnumerable<(string,IMessageProcessor)> processors)
        {
            foreach (var line in receivedMessage.SplitLines())
            {
                if (line.Message.Command.CompareTo("PING", StringComparison.Ordinal) == 0)
                {
                    _receivedPing = true;
                }

                foreach(var botCommand in line.Message.Trailing.ParseBotCommands('!'))
                {
                    foreach (var (command, processor) in processors)
                    {
                        if (command == botCommand)
                        {
                            processor.OnMessage(line.Message, _outMessageQueue.Enqueue);
                        }
                    }
                }

                foreach (var (command, processor) in processors)
                {
                    if (command == "*" && line.Message.Command.CompareTo("PRIVMSG", StringComparison.Ordinal) == 0)
                    {
                        processor.OnMessage(line.Message, _outMessageQueue.Enqueue);
                    }
                }
            }
        }

        public void Dispose()
        {
            _webSocket.Dispose();
        }
    }
}
