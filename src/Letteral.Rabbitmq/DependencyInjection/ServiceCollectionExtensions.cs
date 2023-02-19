using System;
using Letteral.Rabbitmq.Announcement;
using Letteral.Rabbitmq.Connection;
using Letteral.Rabbitmq.Serialization;
using Letteral.Rabbitmq.Subscription;
using Microsoft.Extensions.DependencyInjection;

namespace Letteral.Rabbitmq.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbit(this IServiceCollection services, Action<RabbitOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(setupAction);
        services.Configure(setupAction);
        return services.AddRabbit();
    }

    public static IServiceCollection AddRabbit(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IRabbitSerializer, JsonRabbitSerializer>();
        services.AddSingleton<PersistentConnectionManager>();
        services.AddSingleton<ISubscriptionManager, MemorySubscriptionManager>();
        services.AddScoped<IRabbit, Rabbit>();
        services.AddSingleton<ISubscription, Subscription.Subscription>();
        services.AddScoped<IAnnouncement, Announcement.Announcement>();
        return services;
    }
}