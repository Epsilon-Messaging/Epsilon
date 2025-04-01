using System.Diagnostics.CodeAnalysis;

namespace Common.Models;

[ExcludeFromCodeCoverage]
public record WebsocketMessage<T>(MessageType MessageType, T Data)
{
    public WebsocketMessage() : this(MessageType.MessageResponse, default!)
    {
    }
}