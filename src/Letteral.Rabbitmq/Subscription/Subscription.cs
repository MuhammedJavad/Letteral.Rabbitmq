using System;
using Letteral.Rabbitmq.Connection;
using Letteral.Rabbitmq.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Letteral.Rabbitmq.Subscription
{
    /// <summary>
    /// Subscription object is thread-safe and should be considered as singleton
    /// Using scoped lif-time for Subscription object will cause consumer mismatch on client failures
    /// </summary>
    public class Subscription : ISubscription
    {
        // use a timer for frequently consumes the consumers on connection failure
        private readonly RabbitOptions _options;
        private readonly ILogger<Rabbit> _logger;
        private readonly PersistentConnectionManager _connectionManager;
        private readonly IServiceProvider _serviceResolver;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly object _channelSynchronizer = new();
        private IModel _channel; // channels are not thread-safe

        public Subscription(
            IOptions<RabbitOptions> option,
            ILogger<Rabbit> logger,
            PersistentConnectionManager connectionManager,
            ISubscriptionManager subscriptionManager,
            IServiceProvider serviceResolver)
        {
            _options = option.Value;
            _logger = logger;
            _connectionManager = connectionManager;
            _subscriptionManager = subscriptionManager;
            _serviceResolver = serviceResolver;
            StartConnections();
        }

        public bool Subscribe<TEvent, THandler>(AmqpModel amqp)
            where TEvent : IntegrationEvent
            where THandler : IConsumer<TEvent>
        {
            var consumer = new AsyncEventingConsumer<TEvent, THandler>(Connection.CreateChannel(), _serviceResolver);
            
            if (!TryAddInternalSubscription(amqp, consumer)) return false;

            _logger.LogTrace($"Subscribing to event {nameof(TEvent)} with {nameof(THandler)}");

            return Subscribe(amqp, consumer);
        }

        public bool Unsubscribe(AmqpModel amqp)
        {
            if (!_subscriptionManager.RemoveSubscription(amqp)) return false;

            lock (_channelSynchronizer)
            {
                try
                {
                    _channel.QueueDelete(amqp.GetQueueName(), true);
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Deleting queue returns an error", amqp);
                    return false;
                }
            }
        }

        public void Dispose()
        {
            _channel.Dispose();
            _subscriptionManager.Clear();
        }
        
        private IPersistentConnection Connection => 
            _connectionManager.GetConnection(PersistentConnectionManager.Subscription);
        
        private void StartConnections()
        {
            Connection.CheckConnection();
            _logger.LogTrace("Creating RabbitMQ eventingConsumer channel");
            _channel = Connection.CreateChannel();
            _channel.CallbackException += ChannelOnCallbackException;
        }

        private void ChannelOnCallbackException(object sender, CallbackExceptionEventArgs args)
        {
            // think about exception handling in multi thread envrinomenr
            _logger.LogWarning(args.Exception, "Recreating RabbitMQ eventingConsumer channel");
            DisposeChannel();
            StartConnections();
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

        private bool Subscribe(AmqpModel amqpModel, IAsyncEventingConsumer eventingConsumer)
        {
            lock (_channelSynchronizer)
            {
                try
                {
                    _logger.LogTrace("Declare RabbitMQ Exchange and Queue");

                    _channel.ExchangeDeclare(
                        amqpModel.GetExchangeName(),
                        amqpModel.GetExchangeType(),
                        durable: _options.Durable,
                        autoDelete: _options.AutoDelete);

                    _channel.QueueDeclare(
                        amqpModel.GetQueueName(),
                        durable: _options.Durable,
                        exclusive: false,
                        autoDelete: _options.AutoDelete, 
                        arguments: amqpModel.GetQueueArgs()
                        );

                    _channel.QueueBind(
                        amqpModel.GetQueueName(),
                        amqpModel.GetExchangeName(),
                        amqpModel.GetRoutingKey());

                    _channel.BasicConsume(amqpModel.GetQueueName(), false, eventingConsumer);
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Could not subscribe to the queue", amqpModel);
                    return false;
                }
            }
        }
        
        private void DisposeChannel()
        {
            _channel.CallbackException -= ChannelOnCallbackException;
            _channel.Dispose();
        }
    }
}