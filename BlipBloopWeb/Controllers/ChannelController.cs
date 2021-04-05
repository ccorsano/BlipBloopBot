using BotServiceGrain;
using BotServiceGrainInterface;
using Conceptoire.Twitch.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlipBloopWeb
{
    [Authorize]
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
        public async Task<HelixChannelInfo> GetChannelInfo()
        {
            var channelId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var clusterClient = await _clientProvider.GetConnectedClient();
            var channelGrain = clusterClient.GetGrain<IChannelGrain>(channelId);
            var channelInfo = await channelGrain.GetChannelInfo();

            return channelInfo;
        }

        [HttpGet("activate")]
        public async Task Activate()
        {
            var channelId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            // TODO: once authentication is enabled, remove channelId parameter and use User context
            var clusterClient = await _clientProvider.GetConnectedClient();
            var userGrain = clusterClient.GetGrain<IUserGrain>(channelId);
            await userGrain.ActivateChannel();
        }

        [HttpGet("startbot")]
        public async Task<bool> StartBot()
        {
            var channelId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var clusterClient = await _clientProvider.GetConnectedClient();
            var channelGrain = clusterClient.GetGrain<IChannelGrain>(channelId);
            return await channelGrain.SetBotActivation(true);
        }

        [HttpGet("stopbot")]
        public async Task<bool> StopBot()
        {
            var channelId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var clusterClient = await _clientProvider.GetConnectedClient();
            var channelGrain = clusterClient.GetGrain<IChannelGrain>(channelId);
            return await channelGrain.SetBotActivation(false);
        }
    }
}
