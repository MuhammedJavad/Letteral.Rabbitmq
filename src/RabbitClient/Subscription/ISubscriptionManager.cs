using System.Collections.Generic;
using System.Collections.Immutable;
using RabbitClient.Contracts;

namespace RabbitClient.Subscription
{
    public interface ISubscriptionManager
    {
        ImmutableDictionary<AmqpModel, List<IAsyncEventingConsumer>> Handlers { get; }

        /// <summary>
        /// </summary>
        /// <param name="amqpModel"></param>
        /// <param name="consumer"></param>
        /// <returns></returns>
        void AddSubscription(AmqpModel amqpModel, IAsyncEventingConsumer consumer);
        bool RemoveSubscription(AmqpModel amqpModel);
        bool HasSubscriptionsForEvent(AmqpModel amqpModel, IAsyncEventingConsumer consumer);
        void Clear();
    }
}