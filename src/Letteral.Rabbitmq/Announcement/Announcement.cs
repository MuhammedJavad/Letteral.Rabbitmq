using System;
using Letteral.Rabbitmq.Connection;
using Letteral.Rabbitmq.Contracts;
using Letteral.Rabbitmq.Extensions;
using Letteral.Rabbitmq.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Letteral.Rabbitmq.Announcement;

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
    private readonly ChannelProxy _channel;

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
        _channel = new ChannelProxy(Connection);
    }

    public void Publish<TEvent>(EventDocument<TEvent> evt) where TEvent : class
    {
        TryDeclareExchangeForEvent(evt.Amqp);
        TrySend(evt);
    }

    private PersistentConnection Connection => _connectionManager.GetConnection(1);

    private void TryDeclareExchangeForEvent(AmqpModel amqp)
    {
        try
        {
            _channel.GetOrCreateChannel().DeclarePath(_options, amqp);
        }
        catch (Exception e)
        {
            _logger.LogError(e, 
                "An error occurred while trying to declare exchange. {ExchangeName} {ExchangeType}",
                amqp.GetExchangeName(),
                amqp.GetExchangeType());
            throw;
        }
    }

    private void TrySend<TEvent>(EventDocument<TEvent> evt) where TEvent : class
    {
        try
        {
            var payload = _serialize.Serialize(evt.Event);
            _channel.GetOrCreateChannel().BasicPublish(
                evt.Amqp.GetExchangeName(),
                evt.Amqp.GetRoutingKey(),
                basicProperties: evt.Build(_channel.GetOrCreateChannel()),
                mandatory: true,
                body: payload);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while trying to send the message. {EventId}", evt.Id.ToString());
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}