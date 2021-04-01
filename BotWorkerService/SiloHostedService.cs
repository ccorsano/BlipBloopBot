using BotServiceGrainInterface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
               // Use localhost clustering for a single local silo
               .UseRedisClustering(_configuration.GetValue<string>("REDIS_URL"))
               // Configure ClusterId and ServiceId
               .Configure<ClusterOptions>(options =>
               {
                   options.ClusterId = "dev";
                   options.ServiceId = "TwitchServices";
               })
               .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ChannelGrain).Assembly).WithReferences())
               // Configure connectivity
               .ConfigureEndpoints(hostname: hostname, siloPort: 11111, gatewayPort: 30000);
            _siloHost = builder.Build();
        }

        public Task StartAsync(CancellationToken cancellationToken) => _siloHost.StartAsync();

        public Task StopAsync(CancellationToken cancellationToken) => _siloHost.StopAsync();
    }
}
