using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Conceptoire.Twitch.IRC
{
    public interface ITwitchChatClient : IDisposable
    {
        public Task ConnectAsync(CancellationToken cancellationToken);

        public Task JoinAsync(string channelName, CancellationToken cancellationToken);

        public Task SendCommandAsync(string cmd, string message, CancellationToken cancellationToken);

        public Task SendMessageAsync(OutgoingMessage message, CancellationToken cancellationToken);

        public Task ReceiveIRCMessage(IEnumerable<IMessageProcessor> processors, CancellationToken cancellationToken);
    }
}