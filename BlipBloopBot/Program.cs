using Microsoft.Extensions.Configuration;
using BlipBloopBot.Twitch.IRC;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BlipBloopBot.Twitch.API;
using Microsoft.Extensions.Logging;
using BlipBloopBot.Options;
using Microsoft.Extensions.Caching.Memory;
using BlipBloopBot.Commands;
using BlipBloopBot.Extensions;
using BlipBloopBot.Twitch;
using BlipBloopBot.Storage;
using BlipBloopCommands.Commands.GameSynopsis;

namespace BlipBloopBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration configuration = null;
            var builder = new HostBuilder()
                .ConfigureLogging(configure =>
                {
                    configure.AddConsole();
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

                    // Configure commands
                    services.AddCommand<GameSynopsisCommand>("GameSynopsis");
                    services.AddCommand<TracingMessageProcessor>("MessageTracer");

                    // Add hosted chatbot service
                    services.AddHostedService<BotHostedService>();
                })
                .UseConsoleLifetime();

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
