using System;
using System.Collections.Generic;
using System.Net.Http;
using static Conceptoire.Twitch.Constants.TwitchConstants;

namespace Conceptoire.Twitch.Authentication
{
    public class AuthenticationBuilder : IAuthenticationBuilder
    {
        private enum AuthType
        {
            None,
            OAuthToken,
            AppCredentials,
        }

        private AuthType SelectedAuthType = AuthType.None;
        private HashSet<TwitchOAuthScopes> ConfiguredScopes = new HashSet<TwitchOAuthScopes>();
        private string AppClientId;
        private string AppClientSecret;
        private string OAuthToken;
        private HttpMessageHandler HttpMessageHandler;
        private HttpClient HttpClient;
        private IHttpClientFactory HttpClientFactory;
        private IHttpMessageHandlerFactory HttpMessageHandlerFactory;

        public AuthenticationBuilder()
        {
        }

        public IAuthenticated Build()
        {
            switch (SelectedAuthType)
            {
                case AuthType.OAuthToken:
                    return CreateOAuthTokenAuthenticated();
                case AuthType.AppCredentials:
                    return CreateClientCredentialsAuthenticated();
                case AuthType.None:
                default:
                    throw new InvalidOperationException("No authentication type selected");
            }
        }

        public OAuthAuthenticated CreateOAuthTokenAuthenticated()
        {
            HttpClient client = HttpClient;

            if (client == null)
            {
                if (HttpClientFactory != null)
                {
                    client = HttpClientFactory.CreateClient();
                }
                else if (HttpMessageHandler != null)
                {
                    client = new HttpClient(HttpMessageHandler);
                }
                else if (HttpMessageHandlerFactory != null)
                {
                    client = new HttpClient(HttpMessageHandlerFactory.CreateHandler());
                }
                else
                {
                    client = new HttpClient();
                }
            }

            return new OAuthAuthenticated(client, OAuthToken, ConfiguredScopes);
        }

        public ClientCredentialsAuthenticated CreateClientCredentialsAuthenticated()
        {
            HttpClient client = HttpClient;

            if (client == null)
            {
                if (HttpClientFactory != null)
                {
                    client = HttpClientFactory.CreateClient();
                }
                else if (HttpMessageHandler != null)
                {
                    client = new HttpClient(HttpMessageHandler);
                }
                else if (HttpMessageHandlerFactory != null)
                {
                    client = new HttpClient(HttpMessageHandlerFactory.CreateHandler());
                }
                else
                {
                    client = new HttpClient();
                }
            }

            return new ClientCredentialsAuthenticated(client, AppClientId, AppClientSecret, ConfiguredScopes);
        }

        public IAuthenticationBuilder FromAppCredentials(string clientId, string clientSecret)
        {
            SelectedAuthType = AuthType.AppCredentials;
            AppClientId = clientId;
            AppClientSecret = clientSecret;
            return this;
        }

        public IAuthenticationBuilder FromOAuthToken(string oauth)
        {
            SelectedAuthType = AuthType.OAuthToken;
            OAuthToken = oauth;
            return this;
        }

        public IAuthenticationBuilder WithScope(TwitchOAuthScopes scope)
        {
            ConfiguredScopes.Add(scope);
            return this;
        }

        public IAuthenticationBuilder WithHttpMessageHandler(HttpMessageHandler httpMessageHandler)
        {
            HttpMessageHandler = httpMessageHandler;
            return this;
        }

        public IAuthenticationBuilder WithHttpClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
            return this;
        }

        public IAuthenticationBuilder UseHttpClientFactory(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
            return this;
        }

        public IAuthenticationBuilder UseHttpMessageHandlerFactory(IHttpMessageHandlerFactory messageHandlerFactory)
        {
            HttpMessageHandlerFactory = messageHandlerFactory;
            return this;
        }
    }
}
