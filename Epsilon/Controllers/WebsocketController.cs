using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using Epsilon.Handler.WebsocketMessageHandler;
using Epsilon.Models;
using Epsilon.Services.WebsocketStateService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Epsilon.Controllers;

[ApiController]
[Route("api")]
public class WebSocketController : ControllerBase
{
    private readonly IWebsocketStateService _websocketStateService;
    private readonly IWebsocketMessageHandler _websocketMessageHandler;

    public WebSocketController(
        IWebsocketStateService websocketStateService,
        IWebsocketMessageHandler websocketMessageHandler
    )
    {
        _websocketStateService = websocketStateService;
        _websocketMessageHandler = websocketMessageHandler;
    }

    [Route("websocket")]
    public async Task WebSocket()
    {
        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var sessionId = Guid.NewGuid().ToString();

        var buffer = new byte[1024 * 4];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        MessageService.OutgoingMessages.AsObservable()
            .Where(message => message.MessageType == MessageType.MessageResponse)
            .Where(message =>
                message.Data.Username.Equals(_websocketStateService.GetWebsocketState(sessionId).Username))
            .Subscribe(message => _ = SendMessageAsync(message, webSocket));

        while (!result.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _websocketMessageHandler.HandleMessage(message, sessionId);

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        _websocketStateService.DeleteWebsocket(sessionId);
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    private static async Task SendMessageAsync(WebsocketMessage<MessageResponse> websocketMessage, WebSocket webSocket)
    {
        var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(websocketMessage));
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            false,
            CancellationToken.None
        );
    }
}