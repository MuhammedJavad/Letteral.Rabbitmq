using System;
using System.Net.Sockets;
using Letteral.Rabbitmq.Connection;
using Letteral.Rabbitmq.Contracts;
using Letteral.Rabbitmq.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Letteral.Rabbitmq.Announcement
{
    /// <summary>
    /// Announcement is not thread safe and should be called in an scoped life time
    /// Usually in an application does thousands of events publish, so locking the channel for each publish will cause performance issues
    /// </summary>
    public class Announcement : IAnnouncement
    {
        private readonly RabbitOptions _options;
        private readonly ILogger<Rabbit> _logger;
        private readonly PersistentConnectionManager _connectionManager;
        private readonly ISerializer _serialize;
        private readonly IModel _channel;

        public Announcement(
            ILogger<Rabbit> logger, 
            PersistentConnectionManager connectionManager, 
            ISerializer serialize, 
            IOptions<RabbitOptions> options)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _serialize = serialize;
            _options = options.Value;
            _channel = Connection.CreateChannel();
        }
       
        public bool Publish<TEvent>(EventDocument<TEvent> evt) 
            where TEvent : IntegrationEvent
        {
            if (!Connection.CheckConnection()) return false;
            try
            {
                DeclareExchangeForEvent(evt.Amqp);
                Send(evt);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while publishing messsage");
                return false;
            }
        }

        private IPersistentConnection Connection => _connectionManager.GetConnection();
        
        private void DeclareExchangeForEvent(AmqpModel amqp)
        {
            _logger.LogTrace($"Declaring RabbitMQ exchange. {amqp.GetExchangeName()} {amqp.GetExchangeType()}");

            _channel.ExchangeDeclare(
                exchange: amqp.GetExchangeName(),
                type: amqp.GetExchangeType(),
                durable: _options.Durable,
                autoDelete: _options.AutoDelete);
        }

        private void Send<TEvent>(EventDocument<TEvent> evt) 
            where TEvent : IntegrationEvent
        {
            var payload = _serialize.Serialize(evt.Event);

            DefinePolicy(evt).Execute(() =>
            {
                _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", evt.Id);

                _channel.BasicPublish(
                    evt.Amqp.GetExchangeName(),
                    evt.Amqp.GetRoutingKey(),
                    basicProperties: evt.Build(_channel),
                    mandatory: true,
                    body: payload);
            });
        }

        private RetryPolicy DefinePolicy<TEvent>(EventDocument<TEvent> evt)
            where TEvent : IntegrationEvent
        {
            return Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_options.RetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) =>
                    {
                        _logger.LogWarning(ex,
                            "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})",
                            evt.Id, time.TotalSeconds.ToString("N1"),
                            ex.Message);
                    });
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}