using BotServiceGrain;
using BotServiceGrainInterface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlipBloopWeb.Controllers
{
    [Route("/")]
    [Controller]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        private readonly IClientProvider _clientProvider;

        public HomeController(IClientProvider clientProvider)
        {
            _clientProvider = clientProvider;
        }

        public async Task<IActionResult> Index()
        {
            bool isAuthenticated = User.FindFirst("access_token") != null;
            if (isAuthenticated)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                ViewBag.ChannelId = userId;

                var client = await _clientProvider.GetConnectedClient();
                var grain = client.GetGrain<IUserGrain>(userId);
                if (!await grain.SetOAuthToken(User.FindFirst("access_token").Value))
                {
                    throw new Exception("Error validating token");
                }
                var hasActiveChannel = await grain.HasActiveChannel();
                if (hasActiveChannel)
                {
                    var channelGrain = client.GetGrain<IChannelGrain>(userId);
                    ViewBag.HasActiveBot = await channelGrain.IsBotActive();
                }
                else
                {
                    ViewBag.HasActiveBot = false;
                }
                ViewBag.UserName = User.FindFirstValue("preferred_username");
                ViewBag.HasActiveChannel = hasActiveChannel;
            }
            ViewBag.IsAuthenticated = isAuthenticated;

            return View();
        }

        [AllowAnonymous]
        [Route("/signin-oidc-fragment")]
        public IActionResult SigninOIDCFragment()
        {
            return View();
        }

        [Route("/login")]
        public IActionResult Login()
        {
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index")
            };
            return Challenge(authenticationProperties, "twitch");
        }

        [Route("/logout")]
        public async Task<IActionResult> Logout()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index"),
            };
            await Task.WhenAll(HttpContext.SignOutAsync("cookie", properties));

            return RedirectToAction("Index");
        }
    }
}
