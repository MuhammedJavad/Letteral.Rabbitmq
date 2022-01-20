using System;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Letteral.Rabbitmq.Connection
{
    public class PersistentConnectionManager : IDisposable
    {
        internal const byte Announcement = 0; 
        internal const byte Subscription = 1;
        
        private ImmutableArray<IPersistentConnection> _connections;

        public PersistentConnectionManager(IOptions<RabbitOptions> options, ILogger<IPersistentConnection> logger)
        {
            _connections = CreateConnections(options.Value, logger);
        }

        internal IPersistentConnection GetConnection(byte connectionType = Announcement)
        {
            if (_connections.Length > 1)
            {
                return connectionType == Announcement ?
                    _connections[Announcement] : 
                    _connections[Subscription];
            }

            return _connections[Announcement];
        }
        
        private ImmutableArray<IPersistentConnection> CreateConnections(
            RabbitOptions options,
            ILogger<IPersistentConnection> logger)
        {
            var connections = options.UseSecondaryConnectionForConsumers
                ? new IPersistentConnection[2] {CreateConnection(), CreateConnection()}
                : new IPersistentConnection[1] {CreateConnection()};
            return connections.ToImmutableArray();

            IPersistentConnection CreateConnection()
            {
                var conn = new ConnectionFactory()
                {
                    Uri = new Uri(options.RabbitConnection),
                    DispatchConsumersAsync = true
                };
                return new PersistentConnection(conn, logger);
            }
        }

        private bool disposed;
        public void Dispose()
        {
            if (disposed) return;
            foreach (var connection in _connections) connection.Dispose();
            disposed = true;
        }
    }
}