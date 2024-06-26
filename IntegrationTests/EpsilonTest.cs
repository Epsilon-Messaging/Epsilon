using Epsilon.Models;
using Xunit;

namespace IntegrationTests;

public class EpsilonTest
{
    [Fact]
    public async Task Epsilon_ShouldEchoMessagesBack_WhenItReceivesOneFromTheWebsocket()
    {
        var websocket = new WebSocketTestClient();
        await websocket.Connect("ws://localhost:5172/api/websocket");

        await websocket.Send(
            new WebsocketMessage<LoginRequest>(MessageType.LoginRequest, new LoginRequest("Peaches_MLG"))
        );

        await websocket.ReceivedMessages()
            .Should()
            .PushMatchAsync(s => s.Equals(new WebsocketMessage<MessageResponse>(
                MessageType.MessageResponse,
                new MessageResponse("Hello Peaches_MLG", "Peaches_MLG"))
            ), TimeSpan.FromSeconds(10));

        await websocket.Disconnect();
    }
}