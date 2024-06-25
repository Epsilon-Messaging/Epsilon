using System.Net.WebSockets;
using AutoFixture;
using Epsilon.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Epsilon.Tests.Controllers;

public class WebSocketControllerTest
{
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

        _webSocketController = new WebSocketController
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
        var isFirst = true;
        var message = _fixture.CreateMany<byte>(1024).ToArray();
        _mockWebSocket
            .Setup(socket => socket.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArraySegment<byte> buffer, CancellationToken _) =>
            {
                if (!isFirst)
                {
                    return new WebSocketReceiveResult(
                        0,
                        WebSocketMessageType.Close,
                        true,
                        WebSocketCloseStatus.NormalClosure,
                        "Closed"
                    );
                }

                isFirst = false;
                var count = Math.Min(buffer.Count, message.Length);
                Array.Copy(message, buffer.Array!, count);
                return new WebSocketReceiveResult(
                    message.Length,
                    WebSocketMessageType.Text,
                    false
                );
            });

        await _webSocketController.WebSocket();


        _mockWebSocket.Verify(socket =>
            socket.SendAsync(new ArraySegment<byte>(message, 0, message.Length), WebSocketMessageType.Text, false, It.IsAny<CancellationToken>())
        );
        
        _mockWebSocket.Verify(socket =>
            socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", It.IsAny<CancellationToken>())
        );
    }
}