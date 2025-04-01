using Common.Models;

namespace Epsilon.Services.WebsocketStateService;

public interface IWebsocketStateService
{
    void CreateWebsocket(string sessionId);
    WebsocketState GetWebsocketState(string sessionId);
    List<WebsocketState> GetAllActiveWebsockets();
    void SetWebsocketState(string sessionId, WebsocketState websocketState);
    void DeleteWebsocket(string sessionId);
}