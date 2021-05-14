using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrain.Storage
{
    public static class CustomCategoriesStorageFactory
    {
        internal static IGrainStorage Create(IServiceProvider services, string name)
        {
            IOptionsSnapshot<CustomCategoriesStorageOptions> optionsSnapshot = services.GetRequiredService<IOptionsSnapshot<CustomCategoriesStorageOptions>>();
            return ActivatorUtilities.CreateInstance<CustomCategoriesStorage>(services, name, optionsSnapshot.Get(name), services.GetRequiredService<ILogger<CustomCategoriesStorage>>());
        }
    }
}
