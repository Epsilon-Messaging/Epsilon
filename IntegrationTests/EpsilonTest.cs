using Xunit;

namespace IntegrationTests;

public class EpsilonTest
{
    [Fact]
    public async Task Epsilon_ShouldEchoMessagesBack_WhenItReceivesOneFromTheWebsocket()
    {
        var websocket = new WebSocketTestClient();
        await websocket.Connect("ws://localhost:5172/api/websocket");
        await websocket.Send("Hello, World!");
        await websocket.ReceivedMessages()
            .Should()
            .PushMatchAsync(s => s.Equals("Hello, World!"), TimeSpan.FromSeconds(10));
        await websocket.Disconnect();
    }
}
