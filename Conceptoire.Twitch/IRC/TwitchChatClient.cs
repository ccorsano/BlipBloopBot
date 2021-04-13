using Conceptoire.Twitch.Authentication;
using Conceptoire.Twitch.IRC;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    /// <summary>
    /// A small Twitch IRC over WebSocket client.
    /// Aimed to be low-allocation, low-overhead and extensible.
    /// </summary>
    public class TwitchChatClient : ITwitchChatClient
    {
        private readonly IAuthenticated _authenticated;
        private readonly Uri _endpoint;
        private readonly ClientWebSocket _webSocket;
        private readonly byte[] _outBuffer;
        private readonly byte[] _inBuffer;
        private readonly ILogger _logger;
        private string _joinedChannel;
        private bool _receivedPing;

        private ConcurrentQueue<OutgoingMessage> _outMessageQueue = new ConcurrentQueue<OutgoingMessage>();

        internal TwitchChatClient(IAuthenticated authenticated, Uri endpoint, ILogger<TwitchChatClient> logger)
        {
            _authenticated = authenticated;
            _endpoint = endpoint;
            _logger = logger;
            _webSocket = new ClientWebSocket();
            _inBuffer = new byte[8703]; // IRCv3 8191B Tags + 512B message
            _outBuffer = new byte[1024];
        }

        public TwitchChatClient(IAuthenticated authenticated, IOptions<TwitchChatClientOptions> options, ILogger<TwitchChatClient> logger)
            : this(authenticated, new Uri(options.Value.Endpoint), logger) { }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            await _authenticated.AuthenticateAsync();

            _logger.LogInformation("Connecting to WebSocket IRC endpoint through user {login}", _authenticated.Login);

            var wsUri = _endpoint;
            await _webSocket.ConnectAsync(wsUri, cancellationToken);

            await SendCommandAsync("PASS", $"oauth:{_authenticated.Token}", cancellationToken);
            await SendCommandAsync("NICK", _authenticated.Login, cancellationToken);
            // Request IRCv3 Tags capability
            await SendCommandAsync("CAP REQ", ":twitch.tv/tags", cancellationToken);
            await ReceiveIRCMessage(new List<IMessageProcessor>(), cancellationToken);

            _logger.LogInformation("Connected");
        }

        public async Task JoinAsync(string channelName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Joining channel {channelName}", channelName);

            var botUserName = _authenticated.Login.ToLowerInvariant();
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

        public async Task SendMessageAsync(OutgoingMessage message, CancellationToken cancellationToken)
        {
            if (_joinedChannel == null)
            {
                throw new InvalidOperationException("No active channel");
            }

            var replyTag = message.ReplyParentMessage == null ? "" : $"@reply-parent-msg-id={message.ReplyParentMessage} ";
            var sendText = $"{replyTag}PRIVMSG #{_joinedChannel} :{message.Message}";
            var length = Encoding.UTF8.GetBytes(sendText.AsSpan(), _outBuffer.AsSpan());

            await _webSocket.SendAsync(_outBuffer.AsMemory().Slice(0, length), WebSocketMessageType.Text, true, cancellationToken);
        }

        public async Task ReceiveIRCMessage(IEnumerable<IMessageProcessor> processors, CancellationToken cancellationToken)
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

        private void ParseMessage(string receivedMessage, IEnumerable<IMessageProcessor> processors)
        {
            foreach (var line in receivedMessage.SplitLines())
            {
                _logger.LogInformation("> {command} {prefix} {message}", new string(line.Message.Command), new string(line.Message.Prefix) , new string(line.Message.Trailing));

                if (line.Message.Command.CompareTo("PING", StringComparison.Ordinal) == 0)
                {
                    _receivedPing = true;
                }

                foreach (var processor in processors)
                {
                    if (processor.CanHandleMessage(line.Message))
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
