using System.Diagnostics.CodeAnalysis;

namespace Epsilon.Models;

[ExcludeFromCodeCoverage]
public record WebsocketMessage<T>(MessageType MessageType, T Data)
{
    public WebsocketMessage() : this(MessageType.MessageResponse, default!)
    {
    }
}