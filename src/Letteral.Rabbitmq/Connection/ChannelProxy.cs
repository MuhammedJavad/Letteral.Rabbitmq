using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Letteral.Rabbitmq.Connection;

class ChannelProxy : IDisposable
{
    private readonly PersistentConnection _connection;
    private readonly Action<CallbackExceptionEventArgs> _onCallbackException;
    private readonly object _syncRoot = new();
    private bool _isDisposed;
    private IModel _channel;

    public ChannelProxy(PersistentConnection connection, Action<CallbackExceptionEventArgs> onCallbackException = null)
    {
        _connection = connection;
        _onCallbackException = onCallbackException;
    }

    public bool IsConnected => _channel is { IsOpen: true };
    public IModel GetOrCreateChannel()
    {
        lock (_syncRoot)
        {
            if (IsConnected) return _channel;
            ClearChannel();
            _channel = _connection.CreateChannel();
            _channel.ModelShutdown += ChannelOnModelShutdown;
            _channel.CallbackException += ChannelOnCallbackException;
            return _channel;
        }
    }

    private void ChannelOnCallbackException(object sender, CallbackExceptionEventArgs e)
    {
        GetOrCreateChannel();
        _onCallbackException?.Invoke(e);
    }

    private void ChannelOnModelShutdown(object sender, ShutdownEventArgs e)
    {
        // todo;
    }

    private void ClearChannel()
    {
        if (_channel == null) return;
        _channel.ModelShutdown -= ChannelOnModelShutdown;
        _channel.CallbackException -= ChannelOnCallbackException;
        _channel.Dispose();
        _channel = null;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        ClearChannel();
        _isDisposed = true;
    }
}