using BlipBloopBot.Commands;
using BlipBloopCommands.Commands.GameSynopsis;
using BotServiceGrain;
using BotServiceGrainInterface;
using Conceptoire.Twitch.Authentication;
using Conceptoire.Twitch.Extensions;
using Conceptoire.Twitch.IRC;
using Conceptoire.Twitch.Options;
using Conceptoire.Twitch.Steam;
using Conceptoire.Twitch;
using Conceptoire.Twitch.API;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlipBloopBot.Storage;
using Orleans.Persistence;
using BlipBloopCommands.Storage;
using System.Net.Http;

namespace BotWorkerService
{
    public class SiloHostedService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _services;
        private ISiloHost _siloHost;

        public SiloHostedService(IConfiguration configuration)
        {
            _configuration = configuration;
            BuildSiloHost();
        }

        void BuildSiloHost()
        {
            var instrumentationKey = _configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
            var hostname = _configuration.GetValue<string>("HOSTNAME");
            var azureStorageConnectionString = _configuration.GetValue<string>("Storage:AzureStorageConnectionString");
            var redisClusteringUrl = _configuration.GetValue<string>("REDIS_URL");

            var builder = new SiloHostBuilder()
                .ConfigureLogging(loggingBuilder =>
                {
                    if (! string.IsNullOrEmpty(instrumentationKey))
                    {
                        loggingBuilder.AddApplicationInsights(instrumentationKey);
                    }
                    loggingBuilder.AddConsole();
                })
               // Configure ClusterId and ServiceId
               .Configure<ClusterOptions>(options =>
               {
                   options.ClusterId = "dev";
                   options.ServiceId = "TwitchServices";
               })
               .AddStartupTask<LoadConfigurationStartupTask>()
               .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ChannelGrain).Assembly).WithReferences())
               // Configure connectivity
               .ConfigureEndpoints(hostname: hostname, siloPort: 11111, gatewayPort: 30000);

            if (! string.IsNullOrEmpty(azureStorageConnectionString))
            {
                builder.AddAzureTableGrainStorage("profileStore", (AzureTableStorageOptions options) =>
                {
                    options.ConnectionString = azureStorageConnectionString;
                    options.TableName = "profiles";
                    options.UseJson = true;
                    options.IndentJson = false;
                });
                builder.AddAzureTableGrainStorage("channelStore", (AzureTableStorageOptions options) =>
                {
                    options.ConnectionString = azureStorageConnectionString;
                    options.TableName = "channels";
                    options.UseJson = true;
                    options.IndentJson = false;
                });
                builder.AddAzureTableGrainStorage("botSettingsStore", (AzureTableStorageOptions options) =>
                {
                    options.ConnectionString = azureStorageConnectionString;
                    options.TableName = "botsettings";
                    options.UseJson = true;
                    options.IndentJson = false;
                });
            }
            else
            {
                builder.AddMemoryGrainStorage("profileStore");
                builder.AddMemoryGrainStorage("channelStore");
                builder.AddMemoryGrainStorage("botSettingsStore");
            }

            if (!string.IsNullOrEmpty(redisClusteringUrl))
            {
                // Use redis clustering when available
                builder.UseRedisClustering(redisClusteringUrl);
            }
            else
            {
                // Use localhost clustering for a single local silo
                builder.UseLocalhostClustering(11111, 30000, null, "TwitchServices", "dev");
            }

            // Temp

            builder.ConfigureServices((context, services) =>
            {
                // Load channels and command configuration from static json file, and inject
                var channelsConfig = new ConfigurationBuilder().AddJsonFile("channels.json").Build();
                IEnumerable<ChannelOptions> channelOptions = new List<ChannelOptions>();
                channelsConfig.GetSection("channels").Bind(channelOptions);
                services.AddTransient<IEnumerable<ChannelOptions>>((_) => channelOptions);

                // Configure services
                services.AddHttpClient();
                services.Configure<TwitchApplicationOptions>(_configuration.GetSection("twitch"));
                services.Configure<TwitchChatClientOptions>(_configuration.GetSection("twitch").GetSection("IrcOptions"));
                services.Configure<AzureGameLocalizationStoreOptions>(_configuration.GetSection("loc:azure"));
                services.AddSingleton<IMessageProcessor, TracingMessageProcessor>();
                services.AddTransient<TwitchChatClient>();
                services.AddTransient<TwitchAPIClient>();
                services.AddTransient<IGDBClient>();
                services.AddSingleton<IMemoryCache, MemoryCache>();
                services.AddSingleton<SteamStoreClient>();
                services.AddSingleton<IAuthenticated>(s =>
                    Twitch.Authenticate()
                        .FromAppCredentials(
                            s.GetService<IOptions<TwitchApplicationOptions>>().Value.ClientId,
                            s.GetService<IOptions<TwitchApplicationOptions>>().Value.ClientSecret)
                        .Build()
                );
                services.AddTransient<ITwitchCategoryProvider, GrainTwitchCategoryProvider>();
                services.AddSingleton<IGameLocalizationStore, AzureStorageGameLocalizationStore>();
                services.PostConfigure<TwitchChatClientOptions>(options =>
                {
                    var oauth = Twitch.Authenticate()
                        .FromOAuthToken(options.OAuthToken)
                        .Build();
                    var loggerFactory = new LoggerFactory();
                    using(var httpClient = new HttpClient())
                    using(var apiClient = TwitchAPIClient.Create(oauth))
                    {
                        options.TokenInfo = apiClient.ValidateToken().Result;
                    }
                });

                // Configure commands
                services.AddCommand<GameSynopsisCommand>("GameSynopsis");
                services.AddCommand<TracingMessageProcessor>("MessageTracer");
            });
            _siloHost = builder.Build();
        }

        public Task StartAsync(CancellationToken cancellationToken) => _siloHost.StartAsync();

        public Task StopAsync(CancellationToken cancellationToken) => _siloHost.StopAsync();
    }
}
