using System;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace RabbitClient.Serialization
{
    public sealed class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerOptions _serializerOptions;
        // new()
        // {
        //     Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        //     PropertyNameCaseInsensitive = true,
        //     DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        // };

        public JsonSerializer()
        {
            var encoderSettings = new TextEncoderSettings();
            encoderSettings.AllowCharacters('\u0436', '\u0430');
            encoderSettings.AllowRange(UnicodeRanges.All);
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Encoder = JavaScriptEncoder.Create(encoderSettings),
                WriteIndented = true
            };
        }

        public ReadOnlyMemory<byte> Serialize<T>(T evt) where T : class
        {
            return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(evt, _serializerOptions);
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> payload)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(payload.Span, _serializerOptions);
        }
    }
}