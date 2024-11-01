using BotServiceGrain;
using BotServiceGrainInterface;
using Conceptoire.Twitch.Constants;
using Conceptoire.Twitch.Extensions;
using Conceptoire.Twitch.Model.EventSub;
using Conceptoire.Twitch.Options;
using Conceptoire.Twitch;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Orleans;
using Orleans.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using Conceptoire.Twitch.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Conceptoire.Twitch.Authentication;
using BlipBloopBot.Storage;
using BlipBloopCommands.Storage;
using MudBlazor.Services;
using System;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace BlipBloopWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlipBloopWeb", Version = "v1" });
            });
            services.AddApplicationInsightsTelemetry();

            services.AddTransient<IClientBuilder>(services =>
            {
                var builder = new ClientBuilder()
                    // Clustering information
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "dev";
                        options.ServiceId = "TwitchServices";
                    })
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IChannelGrain).Assembly));

                var redisClusteringUrl = Configuration.GetValue<string>("REDIS_URL");
                if (! string.IsNullOrEmpty(redisClusteringUrl))
                {
                    builder.UseRedisClustering(redisClusteringUrl);
                }
                else
                {
                    builder.UseLocalhostClustering();
                }
                return builder;
            });
            services.AddSingleton<IClientProvider, GrainClientProvider>();

            // Load config, mainly the EventSub secret used for registration
            services.Configure<EventSubOptions>(Configuration.GetSection("twitch:EventSub"));
            // Add the EventSub handler instance to DI
            services.AddEventSub();
            // Register an EventSub event Handler for channel.update events
            services.AddEventSubHandler<TwitchEventSubChannelUpdateEvent>(async (context, eventSub) =>
            {
                context.Logger.LogInformation("Received a channel update for {channelName}, streaming {category} - {text}", eventSub.BroadcasterUserName, eventSub.CategoryName, eventSub.Title);

                var grainClientProvider = context.Services.GetRequiredService<IClientProvider>();
                var grainClient = await grainClientProvider.GetConnectedClient();
                var helixInfo = new HelixChannelInfo
                {
                    BroadcasterId = eventSub.BroadcasterUserId,
                    BroadcasterName = eventSub.BroadcasterUserName,
                    BroadcasterLanguage = eventSub.Language,
                    GameId = eventSub.CategoryId,
                    GameName = eventSub.CategoryName,
                    Title = eventSub.Title,
                };
                var channelGrain = grainClient.GetGrain<IChannelGrain>(helixInfo.BroadcasterId);
                await channelGrain.OnChannelUpdate(helixInfo);
            });

            var twitchOptions = Configuration.GetSection("twitch").Get<TwitchApplicationOptions>();

            services.AddTransient<IAuthenticated>(services =>
                Twitch.Authenticate()
                    .FromAppCredentials(twitchOptions.ClientId, twitchOptions.ClientSecret)
                    .Build());
            services.AddSingleton<TwitchAPIClient>();
            services.Configure<AzureGameLocalizationStoreOptions>(Configuration.GetSection("loc:azure"));
            services.AddSingleton<IGameLocalizationStore, AzureStorageGameLocalizationStore>();
            services.AddHttpClient();

            Action<TwitchConstants.TwitchOAuthScopes[], string, OpenIdConnectOptions>  configureTwitchOpenId = (scopes, callbackPath, options) =>
            {
                options.Authority = "https://id.twitch.tv/oauth2";
                //options.MetadataAddress = "https://id.twitch.tv/oauth2/.well-known/openid-configuration";
                options.ClientId = twitchOptions.ClientId;
                options.ClientSecret = twitchOptions.ClientSecret;
                options.ResponseType = "token id_token";
                options.ResponseMode = "form_post";
                options.CallbackPath = callbackPath;
                options.Events.OnRedirectToIdentityProvider = (context) =>
                {
                    context.ProtocolMessage.RedirectUri = context.ProtocolMessage.RedirectUri + "-fragment";
                    return Task.CompletedTask;
                };
                options.Events.OnTokenValidated = (context) =>
                {
                    ClaimsIdentity identity = context.Principal.Identity as ClaimsIdentity;
                    identity.AddClaim(new Claim("access_token", context.ProtocolMessage.AccessToken));
                    identity.AddClaim(new Claim("id_token", context.ProtocolMessage.IdToken));

                    foreach (var scope in scopes)
                    {
                        identity.AddClaim(new Claim("scope", TwitchConstants.ScopesValues[scope]));
                    }

                    return Task.CompletedTask;
                };
                options.Scope.Remove("profile");

                foreach(var scope in scopes)
                {
                    options.Scope.Add(TwitchConstants.ScopesValues[scope]);
                }
            };

            // Identity
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = "cookie";
                    options.DefaultChallengeScheme = "twitch";
                })
                .AddCookie("cookie", options =>
                {
                    options.Events.OnRedirectToLogin = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }

                        return Task.CompletedTask;
                    };
                })
                .AddOpenIdConnect("twitch", "Twitch", options => configureTwitchOpenId(new TwitchConstants.TwitchOAuthScopes[]{
                    TwitchConstants.TwitchOAuthScopes.ChannelReadEditors,
                    TwitchConstants.TwitchOAuthScopes.ModerationRead,
                    TwitchConstants.TwitchOAuthScopes.UserReadBlockedUsers,
                    TwitchConstants.TwitchOAuthScopes.UserReadBroadcast,
                    TwitchConstants.TwitchOAuthScopes.UserReadSubscriptions,
                    TwitchConstants.TwitchOAuthScopes.ChatRead,
                }, "/signin-oidc", options))
                .AddOpenIdConnect("twitchBot", "Twitch - Bot Account", options => configureTwitchOpenId(new TwitchConstants.TwitchOAuthScopes[]{
                    TwitchConstants.TwitchOAuthScopes.ChannelReadEditors,
                    TwitchConstants.TwitchOAuthScopes.ModerationRead,
                    TwitchConstants.TwitchOAuthScopes.UserReadBlockedUsers,
                    TwitchConstants.TwitchOAuthScopes.UserReadBroadcast,
                    TwitchConstants.TwitchOAuthScopes.ChatRead,
                    TwitchConstants.TwitchOAuthScopes.ChatEdit,
                    TwitchConstants.TwitchOAuthScopes.UserEditFollows,
                }, "/signin-oidc-bot", options));

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddMudServices();
            services.AddAuthorizationCore();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlipBloopWeb v1"));
            }

            var fordwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
            };
            fordwardedHeaderOptions.KnownNetworks.Clear();
            fordwardedHeaderOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(fordwardedHeaderOptions);
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Map the EventSub handler to a specific URL (this is the default value BTW)
            app.UseTwitchEventSub("/webhooks/eventsub");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
