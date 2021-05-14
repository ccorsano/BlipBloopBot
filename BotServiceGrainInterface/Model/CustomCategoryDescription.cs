using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrainInterface.Model
{
    [Serializable]
    public struct CategoryKey
    {
        public string TwitchCategoryId;
        public string Locale;
    }

    [Serializable]
    public class CustomCategoryDescription
    {
        public string TwitchCategoryId { get; set; }
        public string Locale { get; set; }
        public string Description { get; set; }
    }
}
