using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using BlipBloopBot.Twitch.IRC;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BlipBloopBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration configuration = null;
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(configure =>
                {
                    configure.AddUserSecrets<Program>();
                    configure.AddEnvironmentVariables();
                    configuration = configure.Build();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient();
                    services.Configure<TwitchApplicationOptions>(configuration.GetSection("twitch"));
                    services.Configure<TwitchChatClientOptions>(configuration.GetSection("twitch").GetSection("IrcOptions"));
                    services.AddSingleton<IMessageProcessor, TracingMessageProcessor>();
                    services.AddTransient<TwitchChatClient>();
                })
                .UseConsoleLifetime();

            var host = builder.Build();

            using(var scope = host.Services.CreateScope())
            {
                var ircClient = scope.ServiceProvider.GetRequiredService<TwitchChatClient>();
                var cancellationSource = new CancellationTokenSource();
                await ircClient.ConnectAsync(cancellationSource.Token);
                await ircClient.SendCommandAsync("JOIN", "#locklear", cancellationSource.Token);
                while (!cancellationSource.IsCancellationRequested)
                {
                    await ircClient.ReceiveIRCMessage(cancellationSource.Token);
                }
            }
        }
    }
}
