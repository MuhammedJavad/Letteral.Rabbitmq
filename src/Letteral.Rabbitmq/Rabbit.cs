using Letteral.Rabbitmq.Announcement;
using Letteral.Rabbitmq.Contracts;
using Letteral.Rabbitmq.Subscription;

namespace Letteral.Rabbitmq
{
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
            where TEvent : IntegrationEvent =>
            _announcement.Publish(evt);

        public bool Subscribe<TEvent, THandler>(AmqpModel metaData)
            where TEvent : IntegrationEvent
            where THandler : IConsumer<TEvent> =>
            _subscription.Subscribe<TEvent, THandler>(metaData);

        public bool Unsubscribe(AmqpModel metaData) => _subscription.Unsubscribe(metaData);
    }
}
