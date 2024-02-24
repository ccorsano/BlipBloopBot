using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace BotServiceGrain.Storage
{
    public static class CustomCategoriesStorageFactory
    {
        internal static CustomCategoriesStorage Create(IServiceProvider services, string name)
        {
            IOptionsSnapshot<CustomCategoriesStorageOptions> optionsSnapshot = services.GetRequiredService<IOptionsSnapshot<CustomCategoriesStorageOptions>>();
            return ActivatorUtilities.CreateInstance<CustomCategoriesStorage>(services, name, optionsSnapshot.Get(name), services.GetRequiredService<ILogger<CustomCategoriesStorage>>());
        }
    }
}
