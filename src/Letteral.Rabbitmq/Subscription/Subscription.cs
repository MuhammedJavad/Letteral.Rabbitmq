using System;
using Letteral.Rabbitmq.Connection;
using Letteral.Rabbitmq.Contracts;
using Letteral.Rabbitmq.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Letteral.Rabbitmq.Subscription;

/// <summary>
/// Subscription object is thread-safe and should be considered as singleton
/// Using scoped lif-time for Subscription object will cause consumer mismatch on client failures
/// </summary>
class Subscription : ISubscription
{
    private readonly RabbitOptions _options;
    private readonly ILogger<Subscription> _logger;
    private readonly PersistentConnectionManager _connectionManager;
    private readonly IServiceProvider _serviceResolver;
    private readonly ISubscriptionManager _subscriptionManager;
    private readonly ChannelProxy _channel;

    public Subscription(
        IOptions<RabbitOptions> option,
        ILogger<Subscription> logger,
        PersistentConnectionManager connectionManager,
        ISubscriptionManager subscriptionManager,
        IServiceProvider serviceResolver)
    {
        _options = option.Value;
        _logger = logger;
        _connectionManager = connectionManager;
        _subscriptionManager = subscriptionManager;
        _serviceResolver = serviceResolver;
        _channel = new ChannelProxy(Connection, ChannelOnCallbackException);
    }

    public bool Subscribe<TEvent, THandler>(AmqpModel amqp)
        where TEvent : class
        where THandler : IConsumer<TEvent>
    {
        var consumer = new AsyncEventingConsumer<TEvent, THandler>(_channel.GetOrCreateChannel(), _serviceResolver);

        if (!TryAddInternalSubscription(amqp, consumer)) return false;

        return Subscribe(amqp, consumer);
    }

    public bool Unsubscribe(AmqpModel amqp)
    {
        if (!_subscriptionManager.RemoveSubscription(amqp)) return false;

        try
        {
            _channel.GetOrCreateChannel().QueueDelete(amqp.GetQueueName(), true);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Deleting queue returns an error. {QueueName}, {ExchangeName}, {RoutingKey}",
                amqp.GetQueueName(), amqp.GetExchangeName(), amqp.GetRoutingKey());
            return false;
        }
    }

    public void Dispose()
    {
        _channel.Dispose();
        _subscriptionManager.Clear();
    }

    private PersistentConnection Connection => _connectionManager.GetConnection(1);
    
    private void ChannelOnCallbackException(CallbackExceptionEventArgs args)
    {
        foreach (var (key, value) in _subscriptionManager.Handlers)
        foreach (var handlerType in value)
            Subscribe(key, handlerType);
    }

    private bool TryAddInternalSubscription(AmqpModel amqpModel, IAsyncEventingConsumer consumer)
    {
        if (_subscriptionManager.HasSubscriptionsForEvent(amqpModel, consumer)) return false;

        _subscriptionManager.AddSubscription(amqpModel, consumer);

        return true;
    }

    private bool Subscribe(AmqpModel amqp, IAsyncEventingConsumer eventingConsumer)
    {
        try
        {
            _logger.LogTrace("Declare RabbitMQ Exchange and Queue");

            _channel.GetOrCreateChannel().DeclarePath(_options, amqp);

            _channel.GetOrCreateChannel().BasicConsume(amqp.GetQueueName(), false, eventingConsumer);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not subscribe to the queue. {QueueName}, {ExchangeName}, {RoutingKey}",
                amqp.GetQueueName(), amqp.GetExchangeName(), amqp.GetRoutingKey());
            return false;
        }
    }
}