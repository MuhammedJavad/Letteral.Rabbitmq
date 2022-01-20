using System;
using System.Threading.Tasks;
using Letteral.Rabbitmq.Contracts;

namespace Letteral.Rabbitmq.IntegrationTest.TestMessages
{
    internal record BasicMessage : IntegrationEvent
    {
        public BasicMessage()
        {
        }

        public BasicMessage(Guid id, DateTime occurredOn)
        {
            Id = id;
            OccurredOn = occurredOn;
        }

        public static BasicMessage New() => new(Guid.NewGuid(), DateTime.Now);
    }
    
    internal class BasicMessageHandler : IConsumer<BasicMessage>
    {
        private readonly TaskCompletionSource<BasicMessage> _source;
        
        
        public BasicMessageHandler(TaskCompletionSource<BasicMessage> source)
        {
            _source = source;
        }

        public Task Handle(BasicMessage request)
        {
            _source.TrySetResult(request);
            return Task.CompletedTask;
        }
    }
}