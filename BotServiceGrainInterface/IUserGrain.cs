using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrainInterface
{
    public interface IUserGrain : IGrainWithGuidKey
    {
        public Task SetOAuthToken(string oauthToken);
    }
}
