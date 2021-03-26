using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BlipBloopBot.Constants.TwitchConstants;

namespace BlipBloopBot.Twitch.Authentication
{
    public interface IAuthenticated
    {
        internal string Token { get; }
        public DateTimeOffset ExpiresAt { get; }
        public bool AutoRenew { get; }
        public TwitchOAuthScopes[] Scopes { get; }
        public Task AuthenticateAsync();
        public Task AuthenticateAsync(CancellationToken cancellationToken);
        public Task AuthenticateMessageAsync(HttpRequestMessage message);
        public Task AuthenticateMessageAsync(HttpRequestMessage message, CancellationToken cancellationToken);
    }
}
