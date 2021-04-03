﻿using Conceptoire.Twitch.IRC;
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
