using Epsilon.Models;
using Epsilon.Services.WebsocketStateService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Epsilon.Handler.WebsocketMessageHandler;

public class WebsocketMessageHandler : IMessageHandler<string>
{
    private readonly IMessageHandler<LoginRequest> _loginRequestHandler;
    private readonly IMessageHandler<MessageRequest> _messageRequestHandler;

    public WebsocketMessageHandler(
        IMessageHandler<LoginRequest> loginRequestHandler,
        IMessageHandler<MessageRequest> messageRequestHandler
    )
    {
        _loginRequestHandler = loginRequestHandler;
        _messageRequestHandler = messageRequestHandler;
    }

    public void HandleMessage(string? message, string sessionId)
    {
        try
        {
            if (message == null) return;
            var jsonObject = JObject.Parse(message);
            var typeString = jsonObject["MessageType"]?.ToString();

            if (!Enum.TryParse(typeString, true, out MessageType messageType))
            {
                return;
            }

            switch (messageType)
            {
                case MessageType.LoginRequest:
                    _loginRequestHandler.HandleMessage(JsonConvert.DeserializeObject<WebsocketMessage<LoginRequest>>(message)?.Data, sessionId);
                    return;
                case MessageType.MessageRequest:
                    _messageRequestHandler.HandleMessage(JsonConvert.DeserializeObject<WebsocketMessage<MessageRequest>>(message)?.Data, sessionId);
                    return;
                default:
                    return;
            }
        }
        catch (JsonException)
        {
            // Do nothing and ignore error
        }
    }
}