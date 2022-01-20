using System;
using Letteral.Rabbitmq.Contracts;

namespace Letteral.Rabbitmq.Announcement
{
    public interface IAnnouncement : IDisposable
    {
        bool Publish<TEvent>(EventDocument<TEvent> evt) 
            where TEvent : IntegrationEvent;
    }
}