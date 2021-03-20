using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlipBloopWeb.Options
{
    public class EventSubOptions
    {
        public Uri WebHookUri { get; set; }
        public string WebHookSecret { get; set; }
    }
}
