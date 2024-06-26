using System.Net.WebSockets;
using System.Reactive.Subjects;
using System.Text;
using AutoFixture;
using Epsilon.Controllers;
using Epsilon.Handler.WebsocketMessageHandler;
using Epsilon.Models;
using Epsilon.Services.WebsocketStateService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
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
        
        _websocketStateService
            .Setup(service => service.GetWebsocketState(It.IsAny<string>()))
            .Returns(_fixture.Create<WebsocketState>());

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
        
        _websocketStateService.Verify(service => service.CreateWebsocket(It.IsAny<string>()) );
        _websocketStateService.Verify(service => service.DeleteWebsocket(It.IsAny<string>()) );

        _mockWebSocket.Verify(socket =>
            socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", It.IsAny<CancellationToken>())
        );
    }

    [Fact]
    public async Task WebSocket_ShouldHandleMessage_WhenWeSendAMessage()
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
        
        _websocketStateService.Verify(service => service.CreateWebsocket(It.IsAny<string>()) );
        _websocketStateService.Verify(service => service.DeleteWebsocket(It.IsAny<string>()) );

        _mockWebSocket.Verify(socket =>
            socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", It.IsAny<CancellationToken>())
        );
    }

    [Fact]
    public async Task WebSocket_ShouldSendMessage_WhenWePushOneToObservable()
    {
        var replay = new ReplaySubject<object>();
        _websocketStateService
            .Setup(service => service.GetWebsocketState(It.IsAny<string>()))
            .Returns(new WebsocketState("", false, replay));

        var message = _fixture.Create<WebsocketMessage<LoginRequest>>();
        replay.OnNext(message);
        
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

        var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        
        _mockWebSocket.Verify(socket =>
            socket.SendAsync(new ArraySegment<byte>(messageBytes, 0, messageBytes.Length), WebSocketMessageType.Text, true, It.IsAny<CancellationToken>())
        );

        _mockWebSocket.Verify(socket =>
            socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", It.IsAny<CancellationToken>())
        );
    }

    [Fact]
    public async Task WebSocket_ShouldNotSendMessage_WhenWeCloseTheWebsocket()
    {
        var replay = new ReplaySubject<object>();
        _websocketStateService
            .Setup(service => service.GetWebsocketState(It.IsAny<string>()))
            .Returns(new WebsocketState("", false, replay));

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
        
        var message = _fixture.Create<WebsocketMessage<LoginRequest>>();
        replay.OnNext(message);

        _mockWebSocket.Verify(socket =>
            socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", It.IsAny<CancellationToken>())
        );
        
        _mockWebSocket.Verify(socket =>
                socket.SendAsync(It.IsAny<ArraySegment<byte>>(), WebSocketMessageType.Text, true, It.IsAny<CancellationToken>()), Times.Never
        );
    }
}