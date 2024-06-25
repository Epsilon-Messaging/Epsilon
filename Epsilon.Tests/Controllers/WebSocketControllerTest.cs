using System.Net.WebSockets;
using System.Text;
using AutoFixture;
using Epsilon.Controllers;
using Epsilon.Handler.WebsocketMessageHandler;
using Epsilon.Services.WebsocketStateService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Epsilon.Tests.Controllers;

public class WebSocketControllerTest
{
    private readonly Mock<IWebsocketMessageHandler> _mockWebsocketMessageHandler = new();
    private readonly Mock<IWebsocketStateService> _websocketStateService = new();
    private readonly Mock<HttpContext> _mockHttpContext = new();
    private readonly Mock<WebSocketManager> _mockWebSocketManager = new();
    private readonly Mock<WebSocket> _mockWebSocket = new();
    private readonly WebSocketController _webSocketController;
    private readonly Fixture _fixture = new();

    public WebSocketControllerTest()
    {
        _mockWebSocketManager
            .Setup(manager => manager.AcceptWebSocketAsync())
            .ReturnsAsync(_mockWebSocket.Object);

        _mockHttpContext
            .Setup(context => context.WebSockets)
            .Returns(_mockWebSocketManager.Object);

        _webSocketController = new WebSocketController(_websocketStateService.Object, _mockWebsocketMessageHandler.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object,
            }
        };
    }

    [Fact]
    public async Task WebSocket_ShouldCloseConnection_WhenCloseMessageSent()
    {
        _mockWebSocket
            .Setup(socket => socket.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebSocketReceiveResult(
                0,
                WebSocketMessageType.Close,
                true,
                WebSocketCloseStatus.NormalClosure,
                "Closed"
            ));

        await _webSocketController.WebSocket();

        _mockWebSocket.Verify(socket =>
            socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", It.IsAny<CancellationToken>())
        );
    }

    [Fact]
    public async Task WebSocket_ShouldEchoMessage_WhenWeSendAMessage()
    {
        var runs = 0;
        var message = _fixture.CreateMany<byte>(1024).ToArray();
        _mockWebSocket
            .Setup(socket => socket.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArraySegment<byte> buffer, CancellationToken _) =>
            {
                if (runs > 5)
                {
                    return new WebSocketReceiveResult(
                        0,
                        WebSocketMessageType.Close,
                        true,
                        WebSocketCloseStatus.NormalClosure,
                        "Closed"
                    );
                }

                runs++;

                var count = Math.Min(buffer.Count, message.Length);
                Array.Copy(message, buffer.Array!, count);
                return new WebSocketReceiveResult(
                    message.Length,
                    WebSocketMessageType.Text,
                    false
                );
            });

        await _webSocketController.WebSocket();

        _mockWebsocketMessageHandler.Verify(handler =>
                handler.HandleMessage(Encoding.UTF8.GetString(message), It.IsAny<string>()), Times.Exactly(6)
        );

        _mockWebSocket.Verify(socket =>
            socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", It.IsAny<CancellationToken>())
        );
    }
}