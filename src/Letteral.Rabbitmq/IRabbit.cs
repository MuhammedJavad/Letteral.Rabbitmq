using Letteral.Rabbitmq.Contracts;

namespace Letteral.Rabbitmq;

public interface IRabbit
{
    void Publish<TEvent>(EventDocument<TEvent> evt) where TEvent : class;
    bool Subscribe<TEvent, THandler>(AmqpModel metaData)
        where TEvent : class
        where THandler : IConsumer<TEvent>;

    bool Unsubscribe(AmqpModel metaData);
}