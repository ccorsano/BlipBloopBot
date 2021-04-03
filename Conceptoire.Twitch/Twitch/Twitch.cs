using BlipBloopBot.Twitch.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Twitch
{
    public static class Twitch
    {
        public static IAuthenticationBuilder Authenticate()
        {
            return new AuthenticationBuilder();
        }
    }
}
