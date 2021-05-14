using BotServiceGrainInterface.Model;
using Conceptoire.Twitch.API;
using Conceptoire.Twitch.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlipBloopWeb.Model
{
    public class CustomCategoryViewModel
    {
        public GameInfo gameInfo;
        public HelixCategoriesSearchEntry helixCategory;
        public CustomCategoryDescription customCategory;
    }
}
