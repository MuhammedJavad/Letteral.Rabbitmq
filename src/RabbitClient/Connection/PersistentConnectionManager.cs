using System;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace RabbitClient.Connection;

class PersistentConnectionManager : IDisposable
{
    private readonly ImmutableArray<IPersistentConnection> _connections;

    public PersistentConnectionManager(IOptions<RabbitOptions> options, ILogger logger)
    {
        _connections = CreateConnections(options.Value, logger);
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

    private ImmutableArray<IPersistentConnection> CreateConnections(RabbitOptions options, ILogger logger)
    {
        var connections = options.UseSecondaryConnectionForConsumers
            ? new[] { CreateConnection(), CreateConnection() }
            : new[] { CreateConnection() };
        return connections.ToImmutableArray();

        IPersistentConnection CreateConnection()
        {
            var conn = new ConnectionFactory
            {
                Uri = new Uri(options.RabbitConnection),
                UserName = options.UserName,
                Password = options.Password,
                VirtualHost = options.Vhost,
                DispatchConsumersAsync = true,
                UseBackgroundThreadsForIO = true
            };
            return new PersistentConnection(conn, logger);
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