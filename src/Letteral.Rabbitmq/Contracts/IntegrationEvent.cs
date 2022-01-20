using System;

namespace Letteral.Rabbitmq.Contracts
{
    public abstract record IntegrationEvent
    {
        public Guid Id { get; set; }
        public DateTime OccurredOn { get; set; }
    }
}
