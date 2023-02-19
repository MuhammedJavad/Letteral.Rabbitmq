using System;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Letteral.Rabbitmq.Connection;

class PersistentConnectionManager : IDisposable
{
    private readonly ImmutableArray<PersistentConnection> _persistentConnections;
    public PersistentConnectionManager(IServiceProvider provider)
    {
        _persistentConnections = CreateConnections(provider);
    }
    private bool _disposed;

    internal PersistentConnection GetConnection(byte connectionType = 0)
    {
        if (_persistentConnections.Length > 1)
        {
            // 0: Announcement
            // 1: Subscription
            return connectionType == 0 ? _persistentConnections[0] : _persistentConnections[1];
        }

        return _persistentConnections[0];
    }

    private ImmutableArray<PersistentConnection> CreateConnections(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<IOptions<RabbitOptions>>().Value;
        var connectionFactory = CreateConnectionFactory();
        var connections = options.UseSecondaryConnectionForConsumers
            ? new[] { CreateConnection(), CreateConnection() }
            : new[] { CreateConnection() };
        return connections.ToImmutableArray();

        ConnectionFactory CreateConnectionFactory()
        {
            return new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
                VirtualHost = options.Vhost,
                DispatchConsumersAsync = true,
                UseBackgroundThreadsForIO = true
            };
        }
        
        PersistentConnection CreateConnection()
        {
            var logger = provider.GetRequiredService<ILogger<PersistentConnection>>();
            return new PersistentConnection(connectionFactory, logger);
        }
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        foreach (var connection in _persistentConnections) connection.Dispose();
        _disposed = true;
    }
}