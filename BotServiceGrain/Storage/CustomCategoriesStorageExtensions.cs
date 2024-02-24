using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Runtime.Hosting;
using System;

namespace BotServiceGrain.Storage
{
    public static class CustomCategoriesStorageExtensions
    {
        public static ISiloBuilder AddCustomCategoriesStorage(this ISiloBuilder builder, string providerName, Action<CustomCategoriesStorageOptions> options)
        {
            return builder.ConfigureServices(services => services.AddCustomCategoriesStorage(providerName, options));
        }

        public static IServiceCollection AddCustomCategoriesStorage(this IServiceCollection services, string providerName, Action<CustomCategoriesStorageOptions> options)
        {
            services.AddOptions<CustomCategoriesStorageOptions>(providerName).Configure(options);
            return services.AddGrainStorage(providerName, CustomCategoriesStorageFactory.Create);
        }
    }
}
