using System;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace RabbitClient.Connection;

class PersistentConnectionManager : IDisposable
{
    private readonly ImmutableArray<IPersistentConnection> _connections;

    public PersistentConnectionManager(IServiceProvider provider)
    {
        _connections = CreateConnections(provider);
    }

    internal IPersistentConnection GetConnection(byte connectionType = 0)
    {
        if (_connections.Length > 1)
        {
            // 0: Announcement
            // 1: Subscription
            return connectionType == 0 ? _connections[0] : _connections[1];
        }

        return _connections[0];
    }

    private ImmutableArray<IPersistentConnection> CreateConnections(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<IOptions<RabbitOptions>>().Value;
        var connectionFactory = CreateConnectionFactory();
        var connections = options.UseSecondaryConnectionForConsumers
            ? new[] { CreateConnection(), CreateConnection() }
            : new[] { CreateConnection() };
        return connections.ToImmutableArray();

        ConnectionFactory CreateConnectionFactory()
        {
            var conn = string.IsNullOrWhiteSpace(options.RabbitConnection)
                ? new ConnectionFactory
                {
                    HostName = options.HostName,
                    UserName = options.UserName,
                    Password = options.Password,
                    VirtualHost = options.Vhost
                }
                : new ConnectionFactory() { Uri = new Uri(options.RabbitConnection) };
            conn.DispatchConsumersAsync = true;
            conn.UseBackgroundThreadsForIO = true;
            return conn;
        }
        
        IPersistentConnection CreateConnection()
        {
            var logger = provider.GetRequiredService<ILogger<PersistentConnection>>();
            return new PersistentConnection(connectionFactory, logger);
        }
    }

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        foreach (var connection in _connections) connection.Dispose();
        _disposed = true;
    }
}