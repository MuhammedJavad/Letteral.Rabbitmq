using System;
using System.Collections.Generic;
using System.Globalization;
using RabbitMQ.Client;

namespace Letteral.Rabbitmq.Contracts;

public sealed class EventDocument<TEvent> where TEvent : class
{
    private readonly Guid _id;
    private byte _deliveryMode = 2;
    private string _expirationInSeconds;
    private IDictionary<string, object> _headers;

    private EventDocument(TEvent @event, AmqpModel amqp)
    {
        Event = @event;
        Amqp = amqp;
        _id = Guid.NewGuid();
        _headers = new Dictionary<string, object>();
    }
    /// <summary>
    /// Id of message
    /// </summary>
    public Guid Id => _id;
    /// <summary>
    /// Your message
    /// </summary>
    public TEvent Event { get; }
    public AmqpModel Amqp { get; }

    /// <summary>
    /// Save message in storage - persistence = 2
    /// Save message in in memory - transient = 1
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public EventDocument<TEvent> SetDeliveryMode(byte mode)
    {
        _deliveryMode = mode;
        return this;
    }

    public EventDocument<TEvent> SetExpiration(TimeSpan span)
    {
        _expirationInSeconds = span.TotalSeconds.ToString(CultureInfo.InvariantCulture);
        return this;
    }

    public EventDocument<TEvent> AddHeader(string key, string value)
    {
        _headers.TryAdd(key, value);
        return this;
    }

    internal IBasicProperties Build(IModel channel)
    {
        var properties = channel.CreateBasicProperties();
        properties.ContentType = "text/plain";
        properties.DeliveryMode = _deliveryMode;
        properties.CorrelationId = Id.ToString();
        properties.Headers = _headers;

        if (!string.IsNullOrWhiteSpace(_expirationInSeconds))
            properties.Expiration = _expirationInSeconds;

        return properties;
    }

    public static EventDocument<TEvent> New(TEvent @event, AmqpModel metaData) => new(@event, metaData);
}