using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace RabbitClient.Contracts
{
    public interface IConsumer<in TEvent> where TEvent : class
    {
        Task Handle(TEvent request, BasicDeliverEventArgs eventArgs);
    }
}
