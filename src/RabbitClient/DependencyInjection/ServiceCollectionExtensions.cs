using System;
using Microsoft.Extensions.DependencyInjection;
using RabbitClient.Announcement;
using RabbitClient.Connection;
using RabbitClient.Serialization;
using RabbitClient.Subscription;

namespace RabbitClient.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbit(this IServiceCollection services, Action<RabbitOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.Configure(setupAction);
            return services.AddRabbit();
        }

        public static IServiceCollection AddRabbit(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            
            services.AddSingleton<ISerializer, JsonSerializer>();
            services.AddSingleton<PersistentConnectionManager>();
            services.AddSingleton<ISubscriptionManager, MemorySubscriptionManager>();
            services.AddScoped<IRabbit, Rabbit>();
            services.AddSingleton<ISubscription, Subscription.Subscription>();
            services.AddScoped<IAnnouncement, Announcement.Announcement>();
            return services;
        }
    }
}