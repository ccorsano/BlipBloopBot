using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlipBloopWeb
{
    public interface IClientProvider
    {
        public Task<IClusterClient> GetConnectedClient();
    }
}
