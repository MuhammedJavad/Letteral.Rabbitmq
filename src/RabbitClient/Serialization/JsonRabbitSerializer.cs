using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace RabbitClient.Serialization;

sealed class JsonRabbitSerializer : IRabbitSerializer
{
    private readonly JsonSerializerOptions _serializerOptions;

    public JsonRabbitSerializer()
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
        return JsonSerializer.SerializeToUtf8Bytes(evt, _serializerOptions);
    }

    public T Deserialize<T>(ReadOnlyMemory<byte> payload)
    {
        return JsonSerializer.Deserialize<T>(payload.Span, _serializerOptions);
    }
}