using System;
using RabbitMQ.Client;

namespace Letteral.Rabbitmq.Connection
{
    public interface IPersistentConnection : IDisposable
    {
        bool IsConnected { get; }
        bool TryConnect();
        bool CheckConnection();
        IModel CreateChannel();
    }
}