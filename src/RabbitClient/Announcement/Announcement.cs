using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitClient.Connection;
using RabbitClient.Contracts;
using RabbitClient.Extensions;
using RabbitClient.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

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

    public bool Publish<TEvent>(EventDocument<TEvent> evt) where TEvent : class
    {
        if (!Connection.CheckConnection()) return false;
        return TryDeclareExchangeForEvent(evt.Amqp) && TrySend(evt);
    }

    private IPersistentConnection Connection => _connectionManager.GetConnection(1);

    private bool TryDeclareExchangeForEvent(AmqpModel amqp)
    {
        _logger.LogTrace("Declaring RabbitMQ exchange. {ExchangeName} {ExchangeType}", amqp.GetExchangeName(), amqp.GetExchangeType());

        try
        {
            _channel.DeclarePath(_options, amqp);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while trying to declare exchange");
            return false;
        }
    }

    private bool TrySend<TEvent>(EventDocument<TEvent> evt) where TEvent : class
    {
        var payload = _serialize.Serialize(evt.Event);

        var result = DefinePolicy(evt).ExecuteAndCapture(() =>
        {
            _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", evt.Id);

            _channel.BasicPublish(
                evt.Amqp.GetExchangeName(),
                evt.Amqp.GetRoutingKey(),
                basicProperties: evt.Build(_channel),
                mandatory: true,
                body: payload);
        });

        if (result.Outcome == OutcomeType.Successful) return true;

        _logger.LogError(result.FinalException, "An error occurred while trying to send the message");
        return false;
    }

    private RetryPolicy DefinePolicy<TEvent>(EventDocument<TEvent> evt) where TEvent : class
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