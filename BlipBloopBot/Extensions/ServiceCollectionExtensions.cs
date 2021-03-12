using BlipBloopBot.Commands;
using BlipBloopBot.Twitch.IRC;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlipBloopBot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register a command to be instanciated through configuration
        /// </summary>
        /// <typeparam name="TMessageProcessor">IMessageProcessor type to register</typeparam>
        /// <param name="services">Builder instance</param>
        /// <param name="processorName">Name to assign the message processor to</param>
        /// <returns></returns>
        public static IServiceCollection AddCommand<TMessageProcessor>(this IServiceCollection services, string processorName) where TMessageProcessor : class, IMessageProcessor
        {
            services.AddSingleton<TMessageProcessor>();
            services.AddSingleton<IMessageProcessor, TMessageProcessor>(services => services.GetRequiredService<TMessageProcessor>());
            services.AddTransient(provider => new CommandRegistration
            {
                Name = processorName,
                Processor = () => provider.GetRequiredService<TMessageProcessor>()
            });

            return services;
        }
    }
}
