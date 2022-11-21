using System;

namespace RabbitClient.Serialization;

public interface IRabbitSerializer
{
    ReadOnlyMemory<byte> Serialize<T>(T evt) where T : class;
    T Deserialize<T>(ReadOnlyMemory<byte> payload); 
}