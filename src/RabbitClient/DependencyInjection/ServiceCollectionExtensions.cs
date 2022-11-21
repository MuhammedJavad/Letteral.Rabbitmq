using System;
using Microsoft.Extensions.DependencyInjection;
using RabbitClient.Announcement;
using RabbitClient.Connection;
using RabbitClient.Serialization;
using RabbitClient.Subscription;

namespace RabbitClient.DependencyInjection;

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