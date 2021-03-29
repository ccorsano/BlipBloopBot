using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static BlipBloopBot.Constants.TwitchConstants;

namespace BlipBloopBot.Twitch.Authentication
{
    public interface IAuthenticationBuilder
    {
        public IAuthenticationBuilder FromOAuthToken(string oauth);
        public IAuthenticationBuilder FromAppCredentials(string clientId, string clientSecret);
        public IAuthenticationBuilder WithScope(TwitchOAuthScopes scope);
        public IAuthenticationBuilder WithHttpMessageHandler(HttpMessageHandler httpMessageHandler);
        public IAuthenticated Build();
    }
}
