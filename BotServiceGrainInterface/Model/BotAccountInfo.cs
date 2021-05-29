using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrainInterface.Model
{
    [Serializable]
    public class BotAccountInfo
    {
        public bool IsActive { get; set; }
        public string UserId { get; set; }
        public string UserLogin { get; set; }
    }
}
