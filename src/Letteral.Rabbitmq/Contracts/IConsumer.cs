using System.Threading.Tasks;

namespace Letteral.Rabbitmq.Contracts
{
    public interface IConsumer<in TEvent> where TEvent : IntegrationEvent
    {
        Task Handle(TEvent request);
    }
}
