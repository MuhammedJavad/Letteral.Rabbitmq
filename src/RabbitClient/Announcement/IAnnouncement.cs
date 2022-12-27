using System;
using RabbitClient.Contracts;

namespace RabbitClient.Announcement;

public interface IAnnouncement : IDisposable
{
    void Publish<TEvent>(EventDocument<TEvent> evt) where TEvent : class;
}