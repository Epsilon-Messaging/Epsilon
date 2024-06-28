using Epsilon.Models;
using Epsilon.Services.WebsocketStateService;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Epsilon.Handler.WebsocketMessageHandler;

public class LoginRequestMessageHandler : IMessageHandler<LoginRequest>
{
    private readonly IWebsocketStateService _websocketStateService;
    private readonly ILogger _logger = Log.ForContext<MessageRequestMessageHandler>();

    public LoginRequestMessageHandler(IWebsocketStateService websocketStateService)
    {
        _websocketStateService = websocketStateService;
    }
    
    public void HandleMessage(LoginRequest? message, string sessionId)
    {
        if (message == null) return;
        _logger.Debug("Received Login request {@LoginRequest} for {SessionID}", message, sessionId);

        _websocketStateService.SetWebsocketState(sessionId, _websocketStateService.GetWebsocketState(sessionId) with
        {
            Username = message!.Username,
            IsLoggedIn = true
        });
    }
}