using BlipBloopBot.Twitch;
using BlipBloopBot.Twitch.API;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TwitchCategoriesCrawler
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
                    configure.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureAppConfiguration(configure =>
                {
                    configure.AddUserSecrets<Program>();
                    configure.AddEnvironmentVariables();
                    configuration = configure.Build();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Configure services
                    services.AddHttpClient();
                    services.Configure<TwitchApplicationOptions>(configuration.GetSection("twitch"));
                    services.AddTransient<TwitchAPIClient>();
                    services.AddTransient<IGDBClient>();
                    services.AddSingleton<IMemoryCache, MemoryCache>();
                    services.AddSingleton<SteamStoreClient>();

                    services.AddHostedService<TwitchCategoriesCrawlerService>();
                })
                .UseConsoleLifetime();

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
