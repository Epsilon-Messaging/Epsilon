using Epsilon.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Epsilon;

public class WebsocketMessageConverter : JsonConverter
{
    private static readonly Dictionary<MessageType, Type> TypeMapping = new()
    {
        { MessageType.LoginRequest, typeof(LoginRequest) },
        { MessageType.LoginResponse, typeof(LoginResponse) },
        { MessageType.MessageResponse, typeof(MessageResponse) }
    };

    public override bool CanConvert(Type objectType)
    {
        return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(WebsocketMessage<>);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var objectType = value.GetType();
        if (!objectType.IsGenericType || objectType.GetGenericTypeDefinition() != typeof(WebsocketMessage<>))
        {
            throw new JsonSerializationException("Expected WebsocketMessage<> object.");
        }

        var typeProperty = objectType.GetProperty("MessageType");
        var dataProperty = objectType.GetProperty("Data");

        if (typeProperty == null || dataProperty == null)
        {
            throw new JsonSerializationException(
                "Expected properties 'MessageType' and 'Data' in WebsocketMessage<> object.");
        }

        var typeValue = typeProperty.GetValue(value);
        var dataValue = dataProperty.GetValue(value);

        writer.WriteStartObject();

        writer.WritePropertyName("message_type");
        writer.WriteValue(typeValue?.ToString());

        writer.WritePropertyName("data");
        serializer.Serialize(writer, dataValue);

        writer.WriteEndObject();
    }

    public override object? ReadJson(
        JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer
    )
    {
        var jsonObject = JObject.Load(reader);
        var typeString = jsonObject["message_type"]?.ToString() ?? "";

        if (!Enum.TryParse(typeString, true, out MessageType messageType))
        {
            throw new Exception("Unknown type");
        }

        if (!TypeMapping.TryGetValue(messageType, out var dataType))
        {
            throw new Exception("No data type mapping for request type");
        }

        var requestType = typeof(WebsocketMessage<>).MakeGenericType(dataType);
        var request = Activator.CreateInstance(requestType);

        var typeProperty = requestType.GetProperty("MessageType");
        typeProperty?.SetValue(request, Enum.Parse(typeof(MessageType), typeString, true));

        var dataProperty = requestType.GetProperty("Data");
        var data = jsonObject["data"]?.ToObject(dataType, serializer);
        dataProperty?.SetValue(request, data);

        return request;
    }
}