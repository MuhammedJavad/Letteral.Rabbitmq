using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace Letteral.Rabbitmq.Contracts;

public interface IConsumer<in TEvent> where TEvent : class
{
    Task Handle(TEvent request, BasicDeliverEventArgs eventArgs);
}