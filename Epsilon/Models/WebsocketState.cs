using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;

namespace Epsilon.Models;


[ExcludeFromCodeCoverage]
public record WebsocketState(string Username, bool IsLoggedIn, ReplaySubject<object> OutgoingMessages);