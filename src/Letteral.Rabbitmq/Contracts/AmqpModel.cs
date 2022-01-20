using System;
using System.Collections.Generic;

namespace Letteral.Rabbitmq.Contracts
{
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

         private readonly TimeSpan? _queueExpiration;
        // private readonly int? _queueLenghtLimit;
        //private readonly int? _queueLenghtLimit;

        private AmqpModel(
            BindingType binding,
            string routingKey,
            string queueName,
            string exchange,
            TimeSpan? queueExpiration = null)
        {
            _binding = binding;
            _routingKey = routingKey;
            _queueName = queueName;
            _exchange = exchange;
            _queueExpiration = queueExpiration;
        }

        public static AmqpModel FanOut(string exchange, string queue)
        {
            if (string.IsNullOrWhiteSpace(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (string.IsNullOrWhiteSpace(queue)) throw new ArgumentNullException(nameof(queue));

            return new(BindingType.Fanout, string.Empty, queue, exchange);
        }

        public static AmqpModel Direct(string exchange, string queue, string routingKey = default)
        {
            if (string.IsNullOrWhiteSpace(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (string.IsNullOrWhiteSpace(queue)) throw new ArgumentNullException(nameof(queue));
            if (string.IsNullOrWhiteSpace(routingKey)) routingKey = queue;

            return new(BindingType.Direct, routingKey, queue, exchange);
        }

        public static AmqpModel Topic(string exchange, string queue, string routingKey)
        {
            if (string.IsNullOrWhiteSpace(exchange)) throw new ArgumentNullException(nameof(exchange));
            if (string.IsNullOrWhiteSpace(queue)) throw new ArgumentNullException(nameof(queue));
            if (string.IsNullOrWhiteSpace(routingKey)) throw new ArgumentNullException(nameof(routingKey));

            return new(BindingType.Topic, routingKey, queue, exchange);
        }

        public string GetExchangeType() => _binding.ToString();
        public string GetRoutingKey() => _routingKey;
        public string GetExchangeName() => _exchange;
        public string GetQueueName() => _queueName;

        internal Dictionary<string, object> GetQueueArgs()
        {
            var args = new Dictionary<string, object>();

            if (_queueExpiration is {TotalMilliseconds: > 0})
            {
                args.Add("x-expires", _queueExpiration.Value.TotalMilliseconds);
            }

            // if (_queueLenghtLimit is > 0)
            // {
            //     args.Add("x-max-length", _queueLenghtLimit);
            // }
            
            return args;
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
}
