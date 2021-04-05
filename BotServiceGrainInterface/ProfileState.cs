using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrainInterface
{
    [Serializable]
    public class ProfileState
    {
        public bool HasActiveChannel { get; set; } = false;
        public string OAuthToken { get; set; }
    }
}
