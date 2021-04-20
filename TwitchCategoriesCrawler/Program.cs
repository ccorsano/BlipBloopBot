using Conceptoire.Twitch.Authentication;
using Conceptoire.Twitch.Steam;
using Conceptoire.Twitch;
using Conceptoire.Twitch.API;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using BlipBloopCommands.Storage;
using BlipBloopBot.Storage;

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
                    services.Configure<AzureGameLocalizationStoreOptions>(configuration.GetSection("loc:azure"));
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
                    services.AddTransient<IGameLocalizationStore>(services =>
                    {
                        var options = services.GetRequiredService<IOptions<AzureGameLocalizationStoreOptions>>();
                        if (string.IsNullOrEmpty(options.Value.StorageConnectionString) || string.IsNullOrEmpty(options.Value.TableName))
                        {
                            return null;
                        }
                        return services.GetRequiredService<AzureStorageGameLocalizationStore>();
                    });
                    services.AddSingleton<AzureStorageGameLocalizationStore>();
                })
                .UseConsoleLifetime();

            await builder.RunCommandLineApplicationAsync<Tool>(args);
        }
    }
}
