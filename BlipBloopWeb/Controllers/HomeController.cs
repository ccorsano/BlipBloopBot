using BotServiceGrainInterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlipBloopWeb.Controllers
{
    [Route("/")]
    [Controller]
    public class HomeController : Controller
    {
        private readonly IClientProvider _clientProvider;

        public HomeController(IClientProvider clientProvider)
        {
            _clientProvider = clientProvider;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var client = await _clientProvider.GetConnectedClient();
            var grain = client.GetGrain<IUserGrain>(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await grain.SetOAuthToken(User.FindFirst("access_token").Value);
            return View();
        }

        [AllowAnonymous]
        [Route("/signin-oidc-fragment")]
        public IActionResult SigninOIDCFragment()
        {
            return View();
        }
    }
}
