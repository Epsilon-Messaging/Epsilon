using Common;
using Common.Models;
using Epsilon.Services.WebsocketStateService;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Epsilon.Handler.WebsocketMessageHandler;

public class LoginRequestMessageHandler(IWebsocketStateService websocketStateService) : IMessageHandler<LoginRequest>
{
    private readonly ILogger _logger = Log.ForContext<MessageRequestMessageHandler>();

    public void HandleMessage(LoginRequest? message, string sessionId)
    {
        if (message == null) return;
        _logger.Debug("Received Login request {@LoginRequest} for {SessionID}", message, sessionId);

        var (systemPublicKey, systemPrivateKey) = Encryption.GenerateKeys(sessionId, "system@epsilon");

        var newSessionState = websocketStateService.GetWebsocketState(sessionId) with
        {
            Username = message.Username,
            PublicKey = message.PublicKey,
            SystemPrivateKey = systemPrivateKey,
        };

        websocketStateService.SetWebsocketState(sessionId, newSessionState);

        websocketStateService.GetWebsocketState(sessionId).OutgoingMessages.OnNext(new WebsocketMessage<LoginResponse>(
            MessageType.LoginResponse,
            new LoginResponse(newSessionState.ChallangeToken.ToString(), systemPublicKey)
        ));
    }
}