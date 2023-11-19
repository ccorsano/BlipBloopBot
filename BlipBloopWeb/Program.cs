using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlipBloopWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseOrleansClient((context, clientBuilder) =>
                {
                    // Clustering information
                    clientBuilder.Configure<ClusterOptions>(options =>
                     {
                         options.ClusterId = "dev";
                         options.ServiceId = "TwitchServices";
                     });

                    var redisClusteringUrl = context.Configuration.GetValue<string>("REDIS_URL");
                    if (!string.IsNullOrEmpty(redisClusteringUrl))
                    {
                        clientBuilder.UseRedisClustering(redisClusteringUrl);
                    }
                    else
                    {
                        clientBuilder.UseLocalhostClustering();
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
