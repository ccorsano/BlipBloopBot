using BlipBloopWeb;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrainInterface
{
    public class GrainClientProvider : IClientProvider, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Lazy<Task<IClusterClient>> _lazyClient;

        public GrainClientProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _lazyClient = new Lazy<Task<IClusterClient>>(ConnectClient);
        }

        private async Task<IClusterClient> ConnectClient()
        {
            var clientBuilder = _serviceProvider.GetRequiredService<IClientBuilder>();
            var client = clientBuilder.Build();
            await client.Connect();
            return client;
        }

        public void Dispose()
        {
            if (_lazyClient.IsValueCreated && _lazyClient.Value.IsCompletedSuccessfully)
            {
                _lazyClient.Value.Result.Close();
            }
        }

        public Task<IClusterClient> GetConnectedClient()
        {
            if (_lazyClient.IsValueCreated && _lazyClient.Value.IsFaulted)
            {
                
                _lazyClient = new Lazy<Task<IClusterClient>>(ConnectClient);
            }
            return _lazyClient.Value;
        }
    }
}
