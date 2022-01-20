using System;
using Letteral.Rabbitmq.Contracts;

namespace Letteral.Rabbitmq.Serialization
{
    public sealed class JsonSerializer : ISerializer
    {
        public ReadOnlyMemory<byte> Serialize<T>(T evt) where T : IntegrationEvent
        {
            return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(evt);
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> payload)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(payload.Span);
        }
    }
}
