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

            services.AddTransient<IClientBuilder>(services => new ClientBuilder()
                // Clustering information
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "TwitchServices";
                })
                .UseRedisClustering(Configuration.GetValue<string>("REDIS_URL"))
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IChannelGrain).Assembly)));
            services.AddSingleton<IClientProvider, GrainClientProvider>();

            // Load config, mainly the EventSub secret used for registration
            services.Configure<EventSubOptions>(Configuration.GetSection("Twitch:EventSub"));
            // Add the EventSub handler instance to DI
            services.AddEventSub();
            // Register an EventSub event Handler for channel.update events
            services.AddEventSubHandler<TwitchEventSubChannelUpdateEvent>((context, eventSub) =>
            {
                context.Logger.LogInformation("Received a channel update for {channelName}, streaming {category} - {text}", eventSub.BroadcasterUserName, eventSub.CategoryName, eventSub.Title);
                return Task.CompletedTask;
            });

            var twitchOptions = Configuration.GetSection("twitch").Get<TwitchApplicationOptions>();

            // Identity
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = "cookie";
                    options.DefaultChallengeScheme = "twitch";
                })
                .AddCookie("cookie")
                .AddOpenIdConnect("twitch", "Twitch", options =>
                {
                    options.Authority = "https://id.twitch.tv/oauth2";
                    //options.MetadataAddress = "https://id.twitch.tv/oauth2/.well-known/openid-configuration";
                    options.ClientId = twitchOptions.ClientId;
                    options.ClientSecret = twitchOptions.ClientSecret;
                    options.ResponseType = "token id_token";
                    options.ResponseMode = "form_post";
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
                        return Task.CompletedTask;
                    };
                    options.Scope.Remove("profile");
                    options.Scope.Add(TwitchConstants.ScopesValues[TwitchConstants.TwitchOAuthScopes.ChannelReadEditors]);
                    options.Scope.Add(TwitchConstants.ScopesValues[TwitchConstants.TwitchOAuthScopes.ChannelReadHypeTrain]);
                    options.Scope.Add(TwitchConstants.ScopesValues[TwitchConstants.TwitchOAuthScopes.ChannelReadRedemptions]);
                    options.Scope.Add(TwitchConstants.ScopesValues[TwitchConstants.TwitchOAuthScopes.ChannelReadSubscriptions]);
                    options.Scope.Add(TwitchConstants.ScopesValues[TwitchConstants.TwitchOAuthScopes.ModerationRead]);
                    options.Scope.Add(TwitchConstants.ScopesValues[TwitchConstants.TwitchOAuthScopes.UserReadBlockedUsers]);
                    options.Scope.Add(TwitchConstants.ScopesValues[TwitchConstants.TwitchOAuthScopes.UserReadBroadcast]);
                    options.Scope.Add(TwitchConstants.ScopesValues[TwitchConstants.TwitchOAuthScopes.UserReadSubscriptions]);
                    options.Scope.Add(TwitchConstants.ScopesValues[TwitchConstants.TwitchOAuthScopes.ChatRead]);
                });

            services.AddMvc();
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

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Map the EventSub handler to a specific URL (this is the default value BTW)
            app.UseTwitchEventSub("/webhooks/eventsub");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
