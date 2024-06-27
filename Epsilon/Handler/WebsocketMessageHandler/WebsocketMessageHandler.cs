using Epsilon.Models;
using Epsilon.Services.WebsocketStateService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Epsilon.Handler.WebsocketMessageHandler;

public class WebsocketMessageHandler : IWebsocketMessageHandler
{
    private readonly ILogger _logger = Log.ForContext<WebsocketMessageHandler>();

    private readonly IWebsocketStateService _websocketStateService;

    public WebsocketMessageHandler(IWebsocketStateService websocketStateService)
    {
        _websocketStateService = websocketStateService;
    }

    public void HandleMessage(string message, string sessionId)
    {
        var jsonObject = JObject.Parse(message);
        var typeString = jsonObject["MessageType"]?.ToString();

        if (!Enum.TryParse(typeString, true, out MessageType messageType))
        {
            return;
        }

        switch (messageType)
        {
            case MessageType.LoginRequest:
                HandleData(JsonConvert.DeserializeObject<WebsocketMessage<LoginRequest>>(message)?.Data, sessionId);
                return;
            case MessageType.MessageRequest:
                HandleData(JsonConvert.DeserializeObject<WebsocketMessage<MessageRequest>>(message)?.Data, sessionId);
                return;
            default:
                throw new Exception($"No valid handler for {messageType}");
        }
    }

    //Move to a new class?
    public void HandleData(LoginRequest? loginRequest, string sessionId)
    {
        if (loginRequest == null) return;
        _logger.Debug("Received Login request {@LoginRequest} for {SessionID}", loginRequest, sessionId);

        _websocketStateService.SetWebsocketState(sessionId, _websocketStateService.GetWebsocketState(sessionId) with
        {
            Username = loginRequest!.Username,
            IsLoggedIn = true
        });
    }

    public void HandleData(MessageRequest? messageRequest, string sessionId)
    {
        if (messageRequest == null) return;
        var sessionState = _websocketStateService.GetWebsocketState(sessionId);
        if (!sessionState.IsLoggedIn)
        {
            return;
        }

        _logger.Debug("Sending message {@MessageRequest}", messageRequest);

        var recipients = _websocketStateService.GetAllActiveWebsockets()
            .Where(state => state.Username == messageRequest.Username)
            .Select(state => state.OutgoingMessages)
            .ToList();

        foreach (var recipient in recipients)
        {
            recipient.OnNext(new WebsocketMessage<MessageResponse>(MessageType.MessageResponse,
                new MessageResponse(messageRequest.Message, sessionState.Username)
            ));
        }
    }
}