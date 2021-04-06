using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlipBloopWeb.Pages
{
    public class LoginModel : PageModel
    {
        public async Task OnGetAsync(string redirectUri)
        {
            await HttpContext.ChallengeAsync("twitch", new AuthenticationProperties { RedirectUri = redirectUri });
        }
    }
}
