using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Letteral.Rabbitmq.Contracts;

public readonly struct AmqpModel : IEquatable<AmqpModel>
{
    /// <summary>
    /// Exchange-Queue binding
    /// </summary>
    private readonly BindingType _binding;
    /// <summary>
    /// Queue routing key
    /// </summary>
    private readonly string _routingKey;
    /// <summary>
    /// Exchange name
    /// </summary>
    private readonly string _exchange;
    /// <summary>
    /// Queue name
    /// </summary>
    private readonly string _queueName;
    private readonly Dictionary<string, object> _exchangeParameters;
    private readonly Dictionary<string, object> _queueParameters;
    private AmqpModel(BindingType binding, string routingKey, string queueName, string exchange)
    {
        _binding = binding;
        _routingKey = routingKey;
        _queueName = queueName;
        _exchange = exchange;
        _exchangeParameters = new();
        _queueParameters = new();
    }

    public static AmqpModel FanOut(string exchange, string queue)
    {
        if (string.IsNullOrWhiteSpace(queue)) throw new ArgumentNullException(nameof(queue));

        return new(BindingType.Fanout, string.Empty, queue, exchange);
    }
    
    public static AmqpModel DefaultExchange(string queue, string routingKey = default)
    {
        return Direct("", queue, routingKey);
    }
    
    public static AmqpModel Direct(string exchange, string queue, string routingKey = default)
    {
        if (string.IsNullOrWhiteSpace(queue)) throw new ArgumentNullException(nameof(queue));
        if (string.IsNullOrWhiteSpace(routingKey)) routingKey = queue;

        return new(BindingType.Direct, routingKey, queue, exchange);
    }
    
    public static AmqpModel Topic(string exchange, string queue, string routingKey)
    {
        if (string.IsNullOrWhiteSpace(queue)) throw new ArgumentNullException(nameof(queue));
        if (string.IsNullOrWhiteSpace(routingKey)) throw new ArgumentNullException(nameof(routingKey));

        return new(BindingType.Topic, routingKey, queue, exchange);
    }

    internal string GetExchangeType() => _binding.ToString();
    internal string GetRoutingKey() => _routingKey;
    internal string GetExchangeName() => _exchange;
    internal string GetQueueName() => _queueName;
    internal ImmutableDictionary<string, object> ExchangeParameters() => _exchangeParameters.ToImmutableDictionary();
    internal ImmutableDictionary<string, object> QueueParameters() => _queueParameters.ToImmutableDictionary();
    public AmqpModel AddDeadLetter(string exchange, string routing)
    {
        if (!string.IsNullOrWhiteSpace(exchange)) _exchangeParameters.TryAdd("x-dead-letter-exchange", exchange);
        if (!string.IsNullOrWhiteSpace(routing)) _queueParameters.TryAdd("x-dead-letter-routing-key", exchange);
        return this;
    }

    public bool Equals(AmqpModel other)
    {
        return _binding.Equals(other._binding) &&
               _routingKey == other._routingKey &&
               _exchange == other._exchange &&
               _queueName == other._queueName;
    }

    public override bool Equals(object obj)
    {
        return obj is AmqpModel other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_binding, _routingKey, _exchange, _queueName);
    }
}