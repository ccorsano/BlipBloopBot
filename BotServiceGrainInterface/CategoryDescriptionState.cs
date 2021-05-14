using BotServiceGrainInterface.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrainInterface
{
    [Serializable]
    public class CategoryDescriptionState
    {
        public Dictionary<CategoryKey, CustomCategoryDescription> Descriptions { get; set; }
    }
}
