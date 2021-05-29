using System.Net.Http;
using static Conceptoire.Twitch.Constants.TwitchConstants;

namespace Conceptoire.Twitch.Authentication
{
    public interface IAuthenticationBuilder
    {
        public IAuthenticationBuilder FromOAuthToken(string oauth);
        public IAuthenticationBuilder FromAppCredentials(string clientId, string clientSecret);
        public IAuthenticationBuilder WithScope(TwitchOAuthScopes scope);
        public IAuthenticationBuilder WithHttpClient(HttpClient httpClient);
        public IAuthenticationBuilder WithHttpMessageHandler(HttpMessageHandler httpMessageHandler);
        public IAuthenticationBuilder UseHttpMessageHandlerFactory(IHttpMessageHandlerFactory messageHandlerFactory);
        public IAuthenticationBuilder UseHttpClientFactory(IHttpClientFactory httpClientFactory);
        public IAuthenticated Build();
    }
}
