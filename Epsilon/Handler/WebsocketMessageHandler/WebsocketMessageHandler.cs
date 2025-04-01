using Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Epsilon.Handler.WebsocketMessageHandler;

public class WebsocketMessageHandler(
    IMessageHandler<LoginRequest> loginRequestHandler,
    IMessageHandler<MessageRequest> messageRequestHandler,
    IMessageHandler<ChallengeRequest> challengeRequestHandler
)
    : IMessageHandler<string>
{
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
                    loginRequestHandler.HandleMessage(
                        JsonConvert.DeserializeObject<WebsocketMessage<LoginRequest>>(message)?.Data, sessionId);
                    return;
                case MessageType.MessageRequest:
                    messageRequestHandler.HandleMessage(
                        JsonConvert.DeserializeObject<WebsocketMessage<MessageRequest>>(message)?.Data, sessionId);
                    return;
                case MessageType.ChallengeRequest:
                    challengeRequestHandler.HandleMessage(
                        JsonConvert.DeserializeObject<WebsocketMessage<ChallengeRequest>>(message)?.Data, sessionId);
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