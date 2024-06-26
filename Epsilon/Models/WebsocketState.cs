using System.Reactive.Subjects;

namespace Epsilon.Models;

public record WebsocketState(string Username, bool IsLoggedIn, ReplaySubject<object> OutgoingMessages);