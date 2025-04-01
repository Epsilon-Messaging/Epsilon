using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;

namespace Common.Models;

[ExcludeFromCodeCoverage]
public record WebsocketState(
    string Username,
    string PublicKey,
    string SystemPrivateKey,
    bool IsLoggedIn,
    Guid ChallangeToken,
    ReplaySubject<object> OutgoingMessages)
{
    public WebsocketState() : this("", "", "", false, Guid.NewGuid(), new ReplaySubject<object>())
    {
    }
}