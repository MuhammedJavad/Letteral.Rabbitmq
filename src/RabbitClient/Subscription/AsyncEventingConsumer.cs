using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitClient.Contracts;
using RabbitClient.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitClient.Subscription;

public interface IAsyncEventingConsumer :
    IEquatable<IAsyncEventingConsumer>,
    IBasicConsumer,
    IDisposable
{
}

public class AsyncEventingConsumer<TMessage, THandler> :
    AsyncEventingBasicConsumer,
    IAsyncEventingConsumer
    where TMessage : class
    where THandler : IConsumer<TMessage>
{
    public Type HandlerType => typeof(THandler);
    public Type MessageType => typeof(TMessage);

    private readonly IServiceProvider _serviceResolver;

    public AsyncEventingConsumer(IModel model, IServiceProvider serviceResolver) : base(model)
    {
        _serviceResolver = serviceResolver;
        Received += ConsumerReceived;
    }

    private async Task ConsumerReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        using var scope = _serviceResolver.CreateScope();
        try
        {
            var message = scope.ServiceProvider
                .GetRequiredService<IRabbitSerializer>()
                .Deserialize<TMessage>(eventArgs.Body);
            var handler = scope.ServiceProvider.GetRequiredService<THandler>();
            await handler.Handle(message, eventArgs);
        }
        catch (Exception e)
        {
            scope.ServiceProvider
                .GetRequiredService<ILogger<AsyncEventingConsumer<TMessage, THandler>>>()
                .LogError(e, "An error occurred while calling the handler");
        }
        finally
        {
            Model.BasicAck(eventArgs.DeliveryTag, false);
        }
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        Received -= ConsumerReceived;
        _disposed = true;
    }

    protected bool Equals(AsyncEventingConsumer<TMessage, THandler> other)
    {
        return
            MessageType == other.MessageType &&
            HandlerType == other.HandlerType;
    }

    public bool Equals(IAsyncEventingConsumer other) => Equals(this, other);

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == this.GetType() && Equals((AsyncEventingConsumer<TMessage, THandler>)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_serviceResolver, HandlerType);
    }
}