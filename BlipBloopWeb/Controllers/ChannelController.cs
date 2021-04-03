﻿using BlipBloopBot.Twitch.API;
using BotServiceGrain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlipBloopWeb
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChannelController : ControllerBase
    {
        private readonly IClientProvider _clientProvider;

        public ChannelController(IClientProvider clientProvider)
        {
            _clientProvider = clientProvider;
        }

        [HttpGet]
        public async Task<HelixChannelInfo> GetChannelInfo(string channelId)
        {
            var clusterClient = await _clientProvider.GetConnectedClient();
            var channelGrain = clusterClient.GetGrain<IChannelGrain>(channelId);
            var channelInfo = await channelGrain.GetChannelInfo();

            return channelInfo;
        }
    }
}