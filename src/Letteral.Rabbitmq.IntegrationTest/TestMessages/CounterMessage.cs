using System;
using System.Threading;
using System.Threading.Tasks;
using Letteral.Rabbitmq.Contracts;

namespace Letteral.Rabbitmq.IntegrationTest.TestMessages
{
    internal record CounterMessage : IntegrationEvent
    {
        public CounterMessage() { }

        public CounterMessage(int requested) : this(Guid.NewGuid(), DateTime.Now, requested)
        {
            
        }

        public CounterMessage(Guid id, DateTime occurredOn, int requested)
        {
            Id = id;
            OccurredOn = occurredOn;
            Requested = requested;
        }

        public int Requested { get; set; }
    }

    internal class CounterMessageHandler : IConsumer<CounterMessage>
    {
        private readonly TaskCompletionSource<int> _source;
        private readonly HandlerHitCounter _counter;

        public CounterMessageHandler(TaskCompletionSource<int> source, HandlerHitCounter counter)
        {
            _source = source;
            _counter = counter;
        }

        public Task Handle(CounterMessage request)
        {
            _counter.Increase();

            if (request.Requested == _counter.Count)
                _source.TrySetResult(_counter.Count);

            return Task.CompletedTask;
        }
    }

    internal class HandlerHitCounter
    {
        private int _count;
        public int Count => _count;

        public void Increase()
        {
            Interlocked.Increment(ref _count);
        }
    }
}
