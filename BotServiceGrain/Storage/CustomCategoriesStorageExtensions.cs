using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotServiceGrain.Storage
{
    public static class CustomCategoriesStorageExtensions
    {
        public static ISiloHostBuilder AddCustomCategoriesStorage(this ISiloHostBuilder builder, string providerName, Action<CustomCategoriesStorageOptions> options)
        {
            return builder.ConfigureServices(services => services.AddCustomCategoriesStorage(providerName, options));
        }

        public static IServiceCollection AddCustomCategoriesStorage(this IServiceCollection services, string providerName, Action<CustomCategoriesStorageOptions> options)
        {
            services.AddOptions<CustomCategoriesStorageOptions>(providerName).Configure(options);
            return services
                .AddSingletonNamedService(providerName, CustomCategoriesStorageFactory.Create)
                .AddSingletonNamedService(providerName, (s, n) => (ILifecycleParticipant<ISiloLifecycle>)s.GetRequiredServiceByName<IGrainStorage>(n));
        }
    }
}
