namespace Epsilon.Handler.WebsocketMessageHandler;

public interface IMessageHandler<in T>
{
    void HandleMessage(T? message, string sessionId);
}