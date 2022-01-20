using Letteral.Rabbitmq.Contracts;

namespace Letteral.Rabbitmq
{
    public interface IRabbit
    {
        bool Publish<TEvent>(EventDocument<TEvent> evt)
            where TEvent : IntegrationEvent;

        bool Subscribe<TEvent, THandler>(AmqpModel metaData)
            where TEvent : IntegrationEvent
            where THandler : IConsumer<TEvent>;

        bool Unsubscribe(AmqpModel metaData);
    }
}
