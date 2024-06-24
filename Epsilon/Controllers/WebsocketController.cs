using Microsoft.AspNetCore.Mvc;

namespace Epsilon.Controllers;

[ApiController]
[Route("api")]
public class WebSocketController : ControllerBase
{
    [Route("websocket")]
    public async Task WebSocket()
    {
        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        
        var buffer = new byte[1024 * 4];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        while (!result.CloseStatus.HasValue)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
}