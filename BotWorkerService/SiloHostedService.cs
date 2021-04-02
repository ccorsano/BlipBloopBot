using BlipBloopBot.Commands;
using BlipBloopBot.Extensions;
using BlipBloopBot.Options;
using BlipBloopBot.Twitch;
using BlipBloopBot.Twitch.API;
using BlipBloopBot.Twitch.Authentication;
using BlipBloopBot.Twitch.IRC;
using BlipBloopCommands.Commands.GameSynopsis;
using BotServiceGrain;
using BotServiceGrainInterface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            var hostname = _configuration.GetValue<string>("HOSTNAME");
            var builder = new SiloHostBuilder()
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                })
               // Use localhost clustering for a single local silo
               .UseRedisClustering(_configuration.GetValue<string>("REDIS_URL"))
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

            builder.AddMemoryGrainStorage("profileStore");

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
