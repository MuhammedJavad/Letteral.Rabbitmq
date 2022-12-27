using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitClient.Connection;
using RabbitClient.Contracts;
using RabbitClient.Extensions;
using RabbitClient.Serialization;
using RabbitMQ.Client;

namespace RabbitClient.Announcement;

/// <summary>
/// Announcement is not thread safe and should be called in an scoped life time
/// Usually in an application does thousands of events publish, so locking the channel for each publish will cause performance issues
/// </summary>
class Announcement : IAnnouncement
{
    private readonly ILogger<Announcement> _logger;
    private readonly RabbitOptions _options;
    private readonly PersistentConnectionManager _connectionManager;
    private readonly IRabbitSerializer _serialize;
    private readonly IModel _channel;

    public Announcement(
        ILogger<Announcement> logger,
        PersistentConnectionManager connectionManager,
        IRabbitSerializer serialize,
        IOptions<RabbitOptions> options)
    {
        _logger = logger;
        _connectionManager = connectionManager;
        _serialize = serialize;
        _options = options.Value;
        _channel = Connection.CreateChannel();
    }

    public void Publish<TEvent>(EventDocument<TEvent> evt) where TEvent : class
    {
        if (!Connection.CheckConnection()) return;
        TryDeclareExchangeForEvent(evt.Amqp);
        TrySend(evt);
    }

    private IPersistentConnection Connection => _connectionManager.GetConnection(1);

    private void TryDeclareExchangeForEvent(AmqpModel amqp)
    {
        _logger.LogTrace("Declaring RabbitMQ exchange. {ExchangeName} {ExchangeType}", amqp.GetExchangeName(),
            amqp.GetExchangeType());

        try
        {
            _channel.DeclarePath(_options, amqp);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while trying to declare exchange");
            throw;
        }
    }

    private void TrySend<TEvent>(EventDocument<TEvent> evt) where TEvent : class
    {
        try
        {
            _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", evt.Id);
            var payload = _serialize.Serialize(evt.Event);
            _channel.BasicPublish(
                evt.Amqp.GetExchangeName(),
                evt.Amqp.GetRoutingKey(),
                basicProperties: evt.Build(_channel),
                mandatory: true,
                body: payload);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while trying to send the message");
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}