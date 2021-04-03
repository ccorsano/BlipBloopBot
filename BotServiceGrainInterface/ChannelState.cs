using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrainInterface
{
    public class ChannelState
    {
        public string LastTitle { get; set; }
        public string LastLanguage { get; set; }
        public string LastCategoryId { get; set; }
        public string LastCategoryName { get; set; }
    }
}
