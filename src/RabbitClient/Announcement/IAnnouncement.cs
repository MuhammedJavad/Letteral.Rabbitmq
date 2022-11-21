using System;
using RabbitClient.Contracts;

namespace RabbitClient.Announcement;

public interface IAnnouncement : IDisposable
{
    bool Publish<TEvent>(EventDocument<TEvent> evt) where TEvent : class;
}