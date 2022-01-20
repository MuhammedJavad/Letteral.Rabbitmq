using System;
using System.Globalization;
using RabbitMQ.Client;

namespace Letteral.Rabbitmq.Contracts
{
    public sealed class EventDocument<TEvent> where TEvent : IntegrationEvent
    {
        private byte _deliveryMode;
        private TimeSpan? _expirationInSeconds;
        
        private EventDocument(TEvent @event, AmqpModel amqp)
        {
            Event = @event;
            Amqp = amqp;
        }
        /// <summary>
        /// Id of message
        /// </summary>
        public Guid Id => Event.Id;
        /// <summary>
        /// Your message
        /// </summary>
        public TEvent Event { get; }
        public AmqpModel Amqp { get; }
        
        public EventDocument<TEvent> SetDeliveryMode(byte mode)
        {
            _deliveryMode = mode;
            return this;
        }
        
        /// <summary>
        /// Specifying message TTL can cause race-condition in RabbitMQ queues 
        /// https://www.rabbitmq.com/ttl.html
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public EventDocument<TEvent> SetExpiration(TimeSpan span)
        {
            _expirationInSeconds = span;
            return this;
        }
        
        internal IBasicProperties Build(IModel channel)
        {
            var properties = channel.CreateBasicProperties();
            properties.ContentType = "text/plain";
            properties.DeliveryMode = _deliveryMode;
            properties.CorrelationId = Id.ToString();
            
            if (_expirationInSeconds is {TotalMilliseconds:>0})
                properties.Expiration = _expirationInSeconds.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);;
            
            return properties;    
        }
        
        public static EventDocument<TEvent> New(TEvent @event, AmqpModel metaData) => new(@event, metaData);
    }
}
