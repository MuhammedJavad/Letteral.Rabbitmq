using System;
using Letteral.Rabbitmq.Contracts;

namespace Letteral.Rabbitmq.Subscription
{
    public interface ISubscription : IDisposable
    {
        bool Subscribe<TEvent, THandler>(AmqpModel amqp)
            where TEvent : IntegrationEvent
            where THandler : IConsumer<TEvent>;

        bool Unsubscribe(AmqpModel metaData);
    }
}