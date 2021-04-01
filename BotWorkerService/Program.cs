using BlipBloopBot.Commands;
using BlipBloopBot.Extensions;
using BlipBloopBot.Options;
using BlipBloopBot.Storage;
using BlipBloopBot.Twitch;
using BlipBloopBot.Twitch.API;
using BlipBloopBot.Twitch.Authentication;
using BlipBloopBot.Twitch.IRC;
using BlipBloopCommands.Commands.GameSynopsis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotWorkerService
{
    class Program
    {
        static Task Main(string[] args) => BuildHost().RunAsync();

        static IHost BuildHost()
        {
            IConfiguration configuration = null;
            var builder = new HostBuilder()
                .ConfigureLogging(configure =>
                {
                })
                .ConfigureAppConfiguration(configure =>
                {
                    configure.AddUserSecrets<Program>();
                    configure.AddJsonFile("appsettings.json", true);
#if DEBUG
                    configure.AddJsonFile("appsettings.Debug.json", true);
#endif
                    configure.AddEnvironmentVariables();
                    configuration = configure.Build();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Load channels and command configuration from static json file, and inject
                    var channelsConfig = new ConfigurationBuilder().AddJsonFile("channels.json").Build();
                    IEnumerable<ChannelOptions> channelOptions = new List<ChannelOptions>();
                    channelsConfig.GetSection("channels").Bind(channelOptions);
                    services.AddTransient<IEnumerable<ChannelOptions>>((_) => channelOptions);

                    // Configure services
                    services.AddHttpClient();
                    services.Configure<TwitchApplicationOptions>(configuration.GetSection("twitch"));
                    services.Configure<TwitchChatClientOptions>(configuration.GetSection("twitch").GetSection("IrcOptions"));
                    services.AddSingleton<IMessageProcessor, TracingMessageProcessor>();
                    services.AddTransient<TwitchChatClient>();
                    services.AddTransient<TwitchAPIClient>();
                    services.AddTransient<IGDBClient>();
                    services.AddSingleton<IMemoryCache, MemoryCache>();
                    services.AddSingleton<SteamStoreClient>();
                    services.AddSingleton<IGameLocalizationStore, EmbeddedGameLocalizationDb>();
                    services.AddTransient<ITwitchCategoryProvider, PollingTwitchCategoryProvider>();
                    services.AddSingleton<IAuthenticated>(s =>
                        Twitch.Authenticate()
                            .FromAppCredentials(
                                s.GetService<IOptions<TwitchApplicationOptions>>().Value.ClientId,
                                s.GetService<IOptions<TwitchApplicationOptions>>().Value.ClientSecret)
                            .Build()
                    );

                    services.AddHostedService<SiloHostedService>();

                    // Configure commands
                    services.AddCommand<GameSynopsisCommand>("GameSynopsis");
                    services.AddCommand<TracingMessageProcessor>("MessageTracer");
                })
                .UseConsoleLifetime();

            return builder.Build();
        }
    }
}
