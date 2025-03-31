using Epsilon.Models;
using Epsilon.Services.WebsocketStateService;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Epsilon.Handler.WebsocketMessageHandler;

public class MessageRequestMessageHandler : IMessageHandler<MessageRequest>
{
    private readonly IWebsocketStateService _websocketStateService;
    private readonly ILogger _logger = Log.ForContext<MessageRequestMessageHandler>();

    public MessageRequestMessageHandler(IWebsocketStateService websocketStateService)
    {
        _websocketStateService = websocketStateService;
    }

    public void HandleMessage(MessageRequest? message, string sessionId)
    {
        if (message == null) return;
        var sessionState = _websocketStateService.GetWebsocketState(sessionId);
        if (!sessionState.IsLoggedIn)
        {
            return;
        }

        _logger.Debug("Sending message {@MessageRequest}", message);

        var recipients = _websocketStateService.GetAllActiveWebsockets()
            .Where(state => state.PublicKey == message.PublicKey)
            .Where(state => state.IsLoggedIn)
            .Select(state => state.OutgoingMessages)
            .ToList();

        foreach (var recipient in recipients)
        {
            recipient.OnNext(new WebsocketMessage<MessageResponse>(MessageType.MessageResponse,
                new MessageResponse(message.Message, sessionState.PublicKey, sessionState.Username)
            ));
        }
    }
}