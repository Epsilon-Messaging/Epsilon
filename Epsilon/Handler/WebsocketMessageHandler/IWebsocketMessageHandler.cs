namespace Epsilon.Handler.WebsocketMessageHandler;

public interface IWebsocketMessageHandler
{
    void HandleMessage(string message, string sessionId);
}