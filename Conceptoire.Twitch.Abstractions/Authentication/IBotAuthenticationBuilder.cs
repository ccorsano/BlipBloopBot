using Conceptoire.Twitch.IRC;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using static Conceptoire.Twitch.Constants.TwitchConstants;

namespace Conceptoire.Twitch.Authentication
{
    public interface IBotAuthenticationBuilder : IAuthenticationBuilder
    {
        public new IBotAuthenticationBuilder FromOAuthToken(string oauth);
        public new IBotAuthenticationBuilder WithScope(TwitchOAuthScopes scope);
        public new IBotAuthenticationBuilder WithHttpClient(HttpClient httpClient);
        public new IBotAuthenticationBuilder WithHttpMessageHandler(HttpMessageHandler httpMessageHandler);
        public new IBotAuthenticationBuilder UseHttpClientFactory(IHttpClientFactory httpClientFactory);
        public new IBotAuthenticationBuilder UseHttpMessageHandlerFactory(IHttpMessageHandlerFactory messageHandlerFactory);
        public new IBotAuthenticated Build();
    }
}
