using System;

namespace Letteral.Rabbitmq
{
    public class RabbitOptions
    {
        public string ServiceName { get; set; }
        public string RabbitConnection { get; set; }
        public bool AutoDelete { get; set; }
        public TimeSpan QueueExpiration { get; set; }
        public bool Durable { get; set; }
        public int RetryCount { get; set; }
        public int CircuitBreakCount { get; set; }
        public bool UseSecondaryConnectionForConsumers { get; set; }
    }
}