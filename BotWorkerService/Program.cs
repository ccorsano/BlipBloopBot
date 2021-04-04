using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
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
                .ConfigureServices(services =>
                {
                    services.AddHostedService<SiloHostedService>();
                })
                .UseConsoleLifetime();

            return builder.Build();
        }
    }
}
