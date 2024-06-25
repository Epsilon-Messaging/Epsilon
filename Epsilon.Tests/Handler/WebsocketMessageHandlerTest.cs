using Epsilon.Handler.WebsocketMessageHandler;
using Epsilon.Models;
using Epsilon.Services.WebsocketStateService;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Epsilon.Tests.Handler;

public class WebsocketMessageHandlerTest
{
    private readonly Mock<IWebsocketStateService> _websocketStateService = new();

    private readonly WebsocketMessageHandler _websocketMessageHandler;

    public WebsocketMessageHandlerTest()
    {
        _websocketMessageHandler = new WebsocketMessageHandler(_websocketStateService.Object);
    }

    [Fact]
    public void a()
    {
        var loginRequest = new WebsocketMessage<LoginRequest>(MessageType.LoginRequest, new LoginRequest("MyUsername"));
        var json = JsonConvert.SerializeObject(loginRequest);
        _websocketMessageHandler.HandleMessage(json, "1");

        _websocketStateService.Verify(service =>
            service.SetWebsocketState("1", new WebsocketState("MyUsername", true))
        );
    }
}