using System;
using RabbitClient.Contracts;

namespace RabbitClient.Subscription;

public interface ISubscription : IDisposable
{
    bool Subscribe<TEvent, THandler>(AmqpModel amqp)
        where TEvent : class
        where THandler : IConsumer<TEvent>;

    bool Unsubscribe(AmqpModel metaData);
}