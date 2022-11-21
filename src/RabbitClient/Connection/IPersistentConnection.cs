using System;
using RabbitMQ.Client;

namespace RabbitClient.Connection;

public interface IPersistentConnection : IDisposable
{
    bool IsConnected { get; }
    bool TryConnect();
    bool CheckConnection();
    IModel CreateChannel();
}