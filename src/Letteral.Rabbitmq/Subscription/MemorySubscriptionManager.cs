using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Letteral.Rabbitmq.Contracts;

namespace Letteral.Rabbitmq.Subscription;

class MemorySubscriptionManager : ISubscriptionManager
{
    private readonly object _locker = new();
    private readonly ConcurrentDictionary<AmqpModel, List<IAsyncEventingConsumer>> _handlers = new();

    public ImmutableDictionary<AmqpModel, List<IAsyncEventingConsumer>> Handlers => 
        _handlers.ToImmutableDictionary();
        
    public void AddSubscription(AmqpModel amqpModel, IAsyncEventingConsumer consumer)
    {
        _handlers.AddOrUpdate(
            amqpModel,
            new List<IAsyncEventingConsumer>() { consumer },
            (s, list) =>
            {
                if (!list.Contains(consumer)) list.Add(consumer);
                return list;
            });
    }

    public bool RemoveSubscription(AmqpModel amqpModel)
    {
        return _handlers.TryRemove(amqpModel, out _);
    }

    public bool HasSubscriptionsForEvent(AmqpModel amqpModel, IAsyncEventingConsumer consumer)
    {
        var handlers = GetHandlersForEvent(amqpModel);
        return handlers.Any() && handlers.Contains(consumer);
    }
        
    public List<IAsyncEventingConsumer> GetHandlersForEvent(AmqpModel amqpModel)
    {
        var exist = _handlers.TryGetValue(amqpModel, out var list);
        return exist ? list : new List<IAsyncEventingConsumer>();
    }

    public void Clear()
    {
        lock (_locker)
        {
            foreach (var value in _handlers.Values)
                value.ForEach(consumer => consumer.Dispose());

            _handlers.Clear();
        }
    }
}