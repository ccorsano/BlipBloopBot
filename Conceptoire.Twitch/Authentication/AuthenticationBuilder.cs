using System;
using System.Collections.Generic;
using System.Net.Http;
using static Conceptoire.Twitch.Constants.TwitchConstants;

namespace Conceptoire.Twitch.Authentication
{
    public class AuthenticationBuilder : IBotAuthenticationBuilder
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

        IAuthenticated IAuthenticationBuilder.Build()
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

        IBotAuthenticated IBotAuthenticationBuilder.Build()
        {
            if (SelectedAuthType != AuthType.OAuthToken)
            {
                throw new InvalidOperationException("Bot Authentication require to use OAuth token");
            }
            return CreateOAuthTokenAuthenticated();
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

        IAuthenticationBuilder IAuthenticationBuilder.FromAppCredentials(string clientId, string clientSecret)
        {
            SelectedAuthType = AuthType.AppCredentials;
            AppClientId = clientId;
            AppClientSecret = clientSecret;
            return this;
        }

        IAuthenticationBuilder IAuthenticationBuilder.FromOAuthToken(string oauth) => FromOAuthToken(oauth);

        public IBotAuthenticationBuilder FromOAuthToken(string oauth)
        {
            SelectedAuthType = AuthType.OAuthToken;
            OAuthToken = oauth;
            return this;
        }

        IAuthenticationBuilder IAuthenticationBuilder.WithScope(TwitchOAuthScopes scope) => WithScope(scope);

        public IBotAuthenticationBuilder WithScope(TwitchOAuthScopes scope)
        {
            ConfiguredScopes.Add(scope);
            return this;
        }

        IAuthenticationBuilder IAuthenticationBuilder.WithHttpMessageHandler(HttpMessageHandler httpMessageHandler) => WithHttpMessageHandler(httpMessageHandler);

        public IBotAuthenticationBuilder WithHttpMessageHandler(HttpMessageHandler httpMessageHandler)
        {
            HttpMessageHandler = httpMessageHandler;
            return this;
        }

        IAuthenticationBuilder IAuthenticationBuilder.WithHttpClient(HttpClient httpClient) => WithHttpClient(httpClient);

        public IBotAuthenticationBuilder WithHttpClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
            return this;
        }

        IAuthenticationBuilder IAuthenticationBuilder.UseHttpClientFactory(IHttpClientFactory httpClientFactory) => UseHttpClientFactory(httpClientFactory);

        public IBotAuthenticationBuilder UseHttpClientFactory(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
            return this;
        }

        IAuthenticationBuilder IAuthenticationBuilder.UseHttpMessageHandlerFactory(IHttpMessageHandlerFactory messageHandlerFactory) => UseHttpMessageHandlerFactory(messageHandlerFactory);

        public IBotAuthenticationBuilder UseHttpMessageHandlerFactory(IHttpMessageHandlerFactory messageHandlerFactory)
        {
            HttpMessageHandlerFactory = messageHandlerFactory;
            return this;
        }
    }
}
