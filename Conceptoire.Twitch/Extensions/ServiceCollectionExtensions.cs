using Conceptoire.Twitch.IRC;
using Conceptoire.Twitch.Model.EventSub;
using Conceptoire.Twitch.EventSub;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Conceptoire.Twitch.Commands;

namespace Conceptoire.Twitch.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register a command to be instanciated through configuration
        /// </summary>
        /// <typeparam name="TMessageProcessor">IMessageProcessor type to register</typeparam>
        /// <param name="services">Builder instance</param>
        /// <param name="name">Name of the command</param>
        /// <returns></returns>
        public static IServiceCollection AddCommand<TMessageProcessor>(this IServiceCollection services, string name)
            where TMessageProcessor : class, IMessageProcessor
            => AddCommand<TMessageProcessor>(services, new CommandMetadata { Name = name });

        /// <summary>
        /// Register a command to be instanciated through configuration
        /// </summary>
        /// <typeparam name="TMessageProcessor">IMessageProcessor type to register</typeparam>
        /// <param name="services">Builder instance</param>
        /// <param name="metadata">Name and description of the command</param>
        /// <returns></returns>
        public static IServiceCollection AddCommand<TMessageProcessor>(this IServiceCollection services, CommandMetadata metadata)
            where TMessageProcessor : class, IMessageProcessor
        {
            services.AddSingleton<TMessageProcessor>();
            services.AddSingleton<IMessageProcessor, TMessageProcessor>(services => services.GetRequiredService<TMessageProcessor>());
            services.AddTransient(provider => new CommandRegistration
            {
                Metadata = metadata,
                Processor = () => provider.GetRequiredService<TMessageProcessor>()
            });

            return services;
        }

        public static IServiceCollection AddEventSub(this IServiceCollection services)
        {
            services.AddSingleton<EventSubHandler>();
            return services;
        }

        public static IServiceCollection AddEventSubHandler<TEventSubType>(this IServiceCollection services, Func<EventSubContext, TEventSubType, Task> handler) where TEventSubType : TwitchEventSubEvent
        {
            services.AddSingleton<IHandlerRegistration, HandlerRegistration<TEventSubType>>(
                services => new HandlerRegistration<TEventSubType>(handler)
            );
            return services;
        }
    }
}
