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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlipBloopBot.Storage;
using BlipBloopCommands.Storage;
using System.Net.Http;
using BlipBloopCommands.Commands;
using BotServiceGrain.Storage;
using Orleans.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using Orleans.Hosting;

namespace BotWorkerService
{
    public class SiloHostedService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private IHost _siloHost;

        public SiloHostedService(IConfiguration configuration)
        {
            _configuration = configuration;
            BuildSiloHost();
        }

        void BuildSiloHost()
        {
            var aiConnectionString = _configuration.GetValue<string>("ApplicationInsights:ConnectionString");
            var hostname = _configuration.GetValue<string>("HOSTNAME");
            var azureStorageConnectionString = _configuration.GetValue<string>("Storage:AzureStorageConnectionString");
            var redisClusteringUrl = _configuration.GetValue<string>("REDIS_URL");

            var hostBuilder = new HostBuilder()
                //.ConfigureLogging(loggingBuilder =>
                //{
                //    if (!string.IsNullOrEmpty(aiConnectionString))
                //    {
                //        loggingBuilder.AddApplicationInsights();
                //    }
                //    loggingBuilder.AddConsole();
                //})
                .ConfigureServices(services =>
                {
                    services.Configure<TelemetryConfiguration>(configuration =>
                     {
                         if (!string.IsNullOrEmpty(aiConnectionString))
                         {
                             configuration.ConnectionString = aiConnectionString;
                         }
                     });
                });
            hostBuilder
               .UseOrleans(builder =>
                {
                    builder
                       .AddStartupTask<LoadConfigurationStartupTask>()
                        // Configure connectivity
                       .ConfigureEndpoints(hostname: hostname, siloPort: 11111, gatewayPort: 30000);

                    if (!string.IsNullOrEmpty(azureStorageConnectionString))
                    {
                        builder.AddAzureTableGrainStorage("profileStore", (AzureTableStorageOptions options) =>
                        {
                            options.ConfigureTableServiceClient(azureStorageConnectionString);
                            options.TableName = "profiles";
                        });
                        builder.AddAzureTableGrainStorage("channelStore", (AzureTableStorageOptions options) =>
                        {
                            options.ConfigureTableServiceClient(azureStorageConnectionString);
                            options.TableName = "channels";
                        });
                        builder.AddAzureTableGrainStorage("botSettingsStore", (AzureTableStorageOptions options) =>
                        {
                            options.ConfigureTableServiceClient(azureStorageConnectionString);
                            options.TableName = "botsettings";
                        });
                        builder.AddCustomCategoriesStorage("customCategoriesStore", (CustomCategoriesStorageOptions options) =>
                        {
                            options.ConnectionString = azureStorageConnectionString;
                            options.TableName = "customcategories";
                        });
                    }
                    else
                    {
                        builder.AddMemoryGrainStorage("profileStore");
                        builder.AddMemoryGrainStorage("channelStore");
                        builder.AddMemoryGrainStorage("botSettingsStore");
                        builder.AddMemoryGrainStorage("customCategoriesStore");
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
                });

            // Temp
            hostBuilder.ConfigureServices((context, services) =>
            {
                // Configure ClusterId and ServiceId
                services.Configure<ClusterOptions>(options =>
                 {
                     options.ClusterId = "dev";
                     options.ServiceId = "TwitchServices";
                 });
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
                    using (var httpClient = new HttpClient())
                    using (var apiClient = TwitchAPIClient.Create(oauth))
                    {
                        options.TokenInfo = apiClient.ValidateToken().Result;
                    }
                });

                // Configure commands
                services.AddCommand<GameSynopsisCommand>("GameSynopsis");
                services.AddCommand<TracingMessageProcessor>("Logger");
                services.AddCommand<ResponseCommandProcessor>("Response");
            });

            _siloHost = hostBuilder.Build();
        }

        public Task StartAsync(CancellationToken cancellationToken) => _siloHost.StartAsync();

        public Task StopAsync(CancellationToken cancellationToken) => _siloHost.StopAsync();
    }
}
