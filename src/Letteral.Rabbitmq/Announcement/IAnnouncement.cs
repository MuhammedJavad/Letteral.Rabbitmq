using System;
using Letteral.Rabbitmq.Contracts;

namespace Letteral.Rabbitmq.Announcement;

public interface IAnnouncement : IDisposable
{
    void Publish<TEvent>(EventDocument<TEvent> evt) where TEvent : class;
}