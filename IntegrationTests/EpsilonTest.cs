using Epsilon.Models;
using Xunit;

namespace IntegrationTests;

public class EpsilonTest
{
    [Fact]
    public async Task Epsilon_ShouldNotSendMessageToUser_WhenWeAreNotLoggedOn()
    {
        var websocket1 = new WebSocketTestClient();
        var websocket2 = new WebSocketTestClient();
        await websocket1.Connect("ws://localhost:5172/api/websocket");
        await websocket2.Connect("ws://localhost:5172/api/websocket");

        await websocket2.Send(
            new WebsocketMessage<LoginRequest>(MessageType.LoginRequest, new LoginRequest("User2"))
        );

        await websocket1.Send(
            new WebsocketMessage<MessageRequest>(MessageType.MessageRequest, new MessageRequest("Hello :)", "User2"))
        );

        await Task.Delay(1000);

        websocket2.ReceivedMessages()
            .Should()
            .NotPush();

        await websocket1.Disconnect();
        await websocket2.Disconnect();
    }

    [Fact]
    public async Task Epsilon_ShouldSendMessageToUser_WhenWeAreLoggedOn()
    {
        var websocket1 = new WebSocketTestClient();
        var websocket2 = new WebSocketTestClient();
        await websocket1.Connect("ws://localhost:5172/api/websocket");
        await websocket2.Connect("ws://localhost:5172/api/websocket");

        await websocket2.Send(
            new WebsocketMessage<LoginRequest>(MessageType.LoginRequest, new LoginRequest("User2"))
        );

        await websocket1.Send(
            new WebsocketMessage<LoginRequest>(MessageType.LoginRequest, new LoginRequest("User1"))
        );

        await websocket1.Send(
            new WebsocketMessage<MessageRequest>(MessageType.MessageRequest, new MessageRequest("Hello :)", "User2"))
        );

        await websocket2.ReceivedMessages()
            .Should()
            .PushMatchAsync(s => s.Equals(new WebsocketMessage<MessageResponse>(
                MessageType.MessageResponse,
                new MessageResponse("Hello :)", "User1"))
            ), TimeSpan.FromSeconds(10));

        await websocket1.Disconnect();
        await websocket2.Disconnect();
    }

    [Fact]
    public async Task Epsilon_ShouldNotSendMessageToUser_WhenUsernamesDontMatch()
    {
        var websocket1 = new WebSocketTestClient();
        var websocket2 = new WebSocketTestClient();
        await websocket1.Connect("ws://localhost:5172/api/websocket");
        await websocket2.Connect("ws://localhost:5172/api/websocket");

        await websocket1.Send(
            new WebsocketMessage<LoginRequest>(MessageType.LoginRequest, new LoginRequest("User1"))
        );

        await websocket2.Send(
            new WebsocketMessage<LoginRequest>(MessageType.LoginRequest, new LoginRequest("User2"))
        );

        await websocket1.Send(
            new WebsocketMessage<MessageRequest>(MessageType.MessageRequest, new MessageRequest("Hello :)", "User3"))
        );

        await Task.Delay(1000);

        websocket2.ReceivedMessages()
            .Should()
            .NotPush();

        await websocket1.Disconnect();
        await websocket2.Disconnect();
    }
}
