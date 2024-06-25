using Epsilon.Models;

namespace Epsilon.Services.WebsocketStateService;

public interface IWebsocketStateService
{
    WebsocketState GetWebsocketState(string sessionId);
    List<WebsocketState> GetAllActiveWebsockets();
    void SetWebsocketState(string sessionId, WebsocketState websocketState);
    void DeleteWebsocket(string sessionId);
}