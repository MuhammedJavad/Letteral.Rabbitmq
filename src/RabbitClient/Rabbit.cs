using RabbitClient.Announcement;
using RabbitClient.Contracts;
using RabbitClient.Subscription;

namespace RabbitClient;

public class Rabbit : IRabbit
{
    private readonly ISubscription _subscription;
    private readonly IAnnouncement _announcement;

    public Rabbit(
        ISubscription subscription,
        IAnnouncement announcement)
    {
        _subscription = subscription;
        _announcement = announcement;
    }

    public bool Publish<TEvent>(EventDocument<TEvent> evt)
        where TEvent : class =>
        _announcement.Publish(evt);

    public bool Subscribe<TEvent, THandler>(AmqpModel metaData)
        where TEvent : class
        where THandler : IConsumer<TEvent> =>
        _subscription.Subscribe<TEvent, THandler>(metaData);

    public bool Unsubscribe(AmqpModel metaData) => _subscription.Unsubscribe(metaData);
}