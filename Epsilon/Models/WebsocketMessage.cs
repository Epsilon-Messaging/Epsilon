namespace Epsilon.Models;

public record WebsocketMessage<T>(MessageType MessageType, T Data)
{
    public WebsocketMessage() : this(MessageType.MessageResponse, default!)
    {
    }
}