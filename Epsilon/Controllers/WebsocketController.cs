using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using Epsilon.Handler.WebsocketMessageHandler;
using Epsilon.Services.WebsocketStateService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Epsilon.Controllers;

[ApiController]
[Route("api")]
public class WebSocketController : ControllerBase
{
    private readonly IWebsocketStateService _websocketStateService;
    private readonly IMessageHandler<string> _websocketMessageHandler;

    private readonly ILogger _logger = Log.ForContext<WebSocketController>();

    public WebSocketController(
        IWebsocketStateService websocketStateService,
        IMessageHandler<string> websocketMessageHandler
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

        _websocketStateService.CreateWebsocket(sessionId);

        var subscription = _websocketStateService.GetWebsocketState(sessionId).OutgoingMessages.AsObservable()
            .Subscribe(message => _ = SendMessageAsync(message, webSocket));

        while (!result.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _websocketMessageHandler.HandleMessage(message, sessionId);

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        _websocketStateService.DeleteWebsocket(sessionId);
        subscription.Dispose();
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

        _logger.Debug("Logging out {SessionID}", sessionId);
    }

    private static async Task SendMessageAsync(object websocketMessage, WebSocket webSocket)
    {
        var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(websocketMessage));
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }
}