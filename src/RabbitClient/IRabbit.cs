using RabbitClient.Contracts;

namespace RabbitClient
{
    public interface IRabbit
    {
        bool Publish<TEvent>(EventDocument<TEvent> evt) where TEvent : class;

        bool Subscribe<TEvent, THandler>(AmqpModel metaData)
            where TEvent : class
            where THandler : IConsumer<TEvent>;

        bool Unsubscribe(AmqpModel metaData);
    }
}