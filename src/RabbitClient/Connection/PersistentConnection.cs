using System;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitClient.Connection;

class PersistentConnection : IPersistentConnection
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger _logger;
    private readonly object _syncRoot = new();
    private IConnection _connection;
    private bool _disposed;

    public PersistentConnection(IConnectionFactory connectionFactory, ILogger logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public bool IsConnected => _connection is not null && _connection.IsOpen && _disposed is not true;

    public bool TryConnect()
    {
        _logger.LogInformation("RabbitMQ Client is trying to connect");
        lock (_syncRoot)
        {
            ClearConnection();
            _connection = _connectionFactory.CreateConnection();
                
            if (IsConnected)
            {
                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;

                _logger.LogInformation(
                    "RabbitMQ Client acquired a persistent connection to '{HostName}' and is subscribed to failure events",
                    _connection.Endpoint.HostName);
                return true;
            }
            _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
            return false;
        }
    }

    public bool CheckConnection() => IsConnected || TryConnect();

    public IModel CreateChannel()
    {
        if (!IsConnected)
        {
            _logger.LogWarning("No RabbitMQ connections are available to perform this action. Trying to re-connect...");
            TryConnect();
            //throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
        }

        return _connection.CreateModel();
    }

    private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
    {
        if (_disposed) return;

        _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

        TryConnect();
    }

    void OnCallbackException(object sender, CallbackExceptionEventArgs e)
    {
        if (_disposed) return;

        _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

        TryConnect();
    }

    void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
    {
        if (_disposed) return;

        _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

        TryConnect();
    }

    public void Dispose()
    {
        if (_disposed) return;
        ClearConnection();
        GC.SuppressFinalize(this);
        _disposed = true;
    }

    private void ClearConnection()
    {
        if (_connection == null) return;

        _connection.ConnectionShutdown -= OnConnectionShutdown;
        _connection.CallbackException -= OnCallbackException;
        _connection.ConnectionBlocked -= OnConnectionBlocked;
        _connection.Dispose();
    }
}