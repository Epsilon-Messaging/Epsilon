using System.Collections.Concurrent;
using Epsilon.Models;

namespace Epsilon.Services.WebsocketStateService;

public class WebsocketStateService : IWebsocketStateService
{
    private readonly ConcurrentDictionary<string, WebsocketState> _websocketStates = new();

    public WebsocketState GetWebsocketState(string sessionId)
    {
        return _websocketStates.TryGetValue(sessionId, out var websocketState)
            ? websocketState
            : new WebsocketState("", false);
    }

    public List<WebsocketState> GetAllActiveWebsockets()
    {
        return _websocketStates.Values.ToList();
    }

    public void SetWebsocketState(string sessionId, WebsocketState websocketState)
    {
        _websocketStates.AddOrUpdate(sessionId, _ => websocketState, (_, _) => websocketState);
    }

    public void DeleteWebsocket(string sessionId)
    {
        _websocketStates.TryRemove(sessionId, out _);
    }
}