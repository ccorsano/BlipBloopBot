﻿using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrainInterface
{
    public interface IUserGrain : IGrainWithStringKey
    {
        public Task<bool> HasActiveChannel();

        public Task<bool> SetOAuthToken(string oauthToken);

        public Task ActivateChannel();
    }
}
