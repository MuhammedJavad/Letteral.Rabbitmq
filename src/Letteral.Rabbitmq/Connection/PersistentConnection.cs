using System;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Letteral.Rabbitmq.Connection;

class PersistentConnection
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<PersistentConnection> _logger;
    private readonly object _syncRoot = new();
    private IConnection _connection;
    private bool _disposed;

    public PersistentConnection(IConnectionFactory connectionFactory, ILogger<PersistentConnection> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    private bool IsConnected => _connection is not null && _connection.IsOpen && _disposed is not true;

    public void Dispose()
    {
        if (_disposed) return;
        ClearConnection();
        GC.SuppressFinalize(this);
        _disposed = true;
    }
    
    private bool TryConnect()
    {
        lock (_syncRoot)
        {
            try
            {
                ClearConnection();
                _connection = _connectionFactory.CreateConnection();

                if (!IsConnected) return false;

                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;

                _logger.LogInformation(
                    "RabbitMQ Client acquired a persistent connection to '{HostName}', '{Port}'",
                    _connection.Endpoint.HostName,
                    _connection.Endpoint.Port.ToString());
                
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "RabbitMQ connections could not be created");
                return false;
            }
        }
    }

    public IModel CreateChannel()
    {
        if (!IsConnected)
        {
            _logger.LogInformation("No RabbitMQ connection is available to create new channel. Trying to re-connect...");
            var connected = TryConnect();
            if (!connected) throw new Exception("Could not create channel");
        }

        return _connection.CreateModel();
    }

    private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
    {
        OnFailure();
    }
    
    private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
    {
        OnFailure();
    }

    private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
    {
        OnFailure();
    }

    private void OnFailure()
    {
        if (_disposed) return;
        _logger.LogError("RabbitMQ connection is shutdown. Trying to re-connect...");
        TryConnect();
    }
    
    private void ClearConnection()
    {
        if (_connection == null) return;

        try
        {
            _connection.ConnectionShutdown -= OnConnectionShutdown;
            _connection.CallbackException -= OnCallbackException;
            _connection.ConnectionBlocked -= OnConnectionBlocked;
            _connection.Dispose();
        }
        finally
        {
            _connection = null;
        }
    }
}