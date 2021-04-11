using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlipBloopWeb.Model
{
    public class ServiceContext
    {
        public bool IsAuthenticated { get; set; }
        public bool IsChannelIntegrationActive { get; set; }
        public bool IsBotRunning { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string OAuthToken { get; set; }
        public string ActiveChannel { get; set; }
    }
}
