using BotServiceGrain;
using Conceptoire.Twitch.API;
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

        [HttpGet("startbot")]
        public async Task<bool> StartBot(string channelId)
        {
            var clusterClient = await _clientProvider.GetConnectedClient();
            var channelGrain = clusterClient.GetGrain<IChannelGrain>(channelId);
            return await channelGrain.SetBotActivation(true);
        }

        [HttpGet("stopbot")]
        public async Task<bool> StopBot(string channelId)
        {
            var clusterClient = await _clientProvider.GetConnectedClient();
            var channelGrain = clusterClient.GetGrain<IChannelGrain>(channelId);
            return await channelGrain.SetBotActivation(false);
        }
    }
}
