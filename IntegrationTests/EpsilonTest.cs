using Xunit;

namespace IntegrationTests;

public class EpsilonTest
{
    [Fact]
    public async Task Epsilon_ShouldEchoMessagesBack_WhenItReceivesOneFromTheWebsocket()
    {
        var websocket = new WebSocketTestClient();
        await websocket.Connect("ws://localhost:5172/api/websocket");
        await websocket.Send("{\"MessageType\":\"LoginRequest\",\"data\":{\"Username\":\"Peaches_MLG\"}}");
        await websocket.ReceivedMessages()
            .Should()
            .PushMatchAsync(s => s.Equals("{\"MessageType\":2,\"Data\":{\"Message\":\"Hello Peaches_MLG\",\"Username\":\"Peaches_MLG\"}}"), TimeSpan.FromSeconds(10));
        await websocket.Disconnect();
    }
}