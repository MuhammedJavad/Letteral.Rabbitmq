using System;
using Letteral.Rabbitmq.Contracts;

namespace Letteral.Rabbitmq.Serialization
{
    public interface ISerializer
    {
        ReadOnlyMemory<byte> Serialize<T>(T evt) where T : IntegrationEvent;
        T Deserialize<T>(ReadOnlyMemory<byte> payload); 
    }
}