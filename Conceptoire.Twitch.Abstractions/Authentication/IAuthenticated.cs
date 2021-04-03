using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Conceptoire.Twitch.Constants.TwitchConstants;

namespace Conceptoire.Twitch.Authentication
{
    public interface IAuthenticated
    {
        public string Token { get; }
        public DateTimeOffset ExpiresAt { get; }
        public bool AutoRenew { get; }
        public TwitchOAuthScopes[] Scopes { get; }
        public string Login { get; }
        public Task AuthenticateAsync();
        public Task AuthenticateAsync(CancellationToken cancellationToken);
        public Task AuthenticateMessageAsync(HttpRequestMessage message);
        public Task AuthenticateMessageAsync(HttpRequestMessage message, CancellationToken cancellationToken);
    }
}
