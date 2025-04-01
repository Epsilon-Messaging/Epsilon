using Common;
using Common.Models;
using Epsilon.Services.WebsocketStateService;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Epsilon.Handler.WebsocketMessageHandler;

public class ChallengeRequestMessageHandler(IWebsocketStateService websocketStateService)
    : IMessageHandler<ChallengeRequest>
{
    private readonly ILogger _logger = Log.ForContext<MessageRequestMessageHandler>();

    public void HandleMessage(ChallengeRequest? message, string sessionId)
    {
        if (message == null) return;
        _logger.Debug("Received Challenge request {@ChallengeRequest} for {SessionID}", message, sessionId);

        var currentState = websocketStateService.GetWebsocketState(sessionId);

        bool success;
        try
        {
            var decrypted = Encryption.DecryptMessage(
                message.SignedChallenge,
                Encryption.ReadPrivateKey(currentState.SystemPrivateKey, sessionId),
                Encryption.ReadPublicKey(currentState.PublicKey)
            );
            success = decrypted == currentState.ChallangeToken.ToString();
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "Failed to decrypt message");
            success = false;
        }

        if (success)
        {
            Console.WriteLine("User Successfully Logged in");
            websocketStateService.SetWebsocketState(sessionId, currentState with
            {
                IsLoggedIn = true
            });
        }

        websocketStateService.GetWebsocketState(sessionId).OutgoingMessages.OnNext(new WebsocketMessage<ChallengeResponse>(
            MessageType.ChallengeResponse,
            new ChallengeResponse(success)
        ));
    }
}