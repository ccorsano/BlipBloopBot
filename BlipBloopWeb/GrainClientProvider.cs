using BlipBloopWeb;
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
        private readonly IClientBuilder _clientBuilder;
        private Lazy<Task<IClusterClient>> _lazyClient;

        public GrainClientProvider(IClientBuilder clientBuilder)
        {
            _clientBuilder = clientBuilder;
            _lazyClient = new Lazy<Task<IClusterClient>>(async () =>
            {
                var client = _clientBuilder.Build();
                await client.Connect();
                return client;
            });
        }

        public void Dispose()
        {
            if (_lazyClient.IsValueCreated && _lazyClient.Value.IsCompleted)
            {
                _lazyClient.Value.Result.Close();
            }
        }

        public Task<IClusterClient> GetConnectedClient()
        {
            return _lazyClient.Value;
        }
    }
}
