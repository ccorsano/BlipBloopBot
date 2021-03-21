using BlipBloopBot.Twitch.EventSub;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseTwitchEventSub(this IApplicationBuilder builder, string path = "/webhooks/eventsub")
        {
            return builder.Map(path, builder =>
            {
                var handler = (EventSubHandler) builder.ApplicationServices.GetService(typeof(EventSubHandler));
                builder.Run(handler.HandleRequestAsync);
            });
        }
    }
}
