using System.Reactive.Subjects;
using AutoFixture;
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
    private readonly Fixture _fixture = new();

    public WebsocketMessageHandlerTest()
    {
        _websocketStateService
            .Setup(service => service.GetWebsocketState(It.IsAny<string>()))
            .Returns(_fixture.Create<WebsocketState>());

        _websocketMessageHandler = new WebsocketMessageHandler(_websocketStateService.Object);
    }

    [Fact]
    public void WebsocketMessageHandler_ShouldSetUsername_WhenWeSendALoginRequest()
    {
        var loginRequest = new WebsocketMessage<LoginRequest>(MessageType.LoginRequest, new LoginRequest("MyUsername"));
        var json = JsonConvert.SerializeObject(loginRequest);
        _websocketMessageHandler.HandleMessage(json, "1");

        _websocketStateService.Verify(service =>
            service.SetWebsocketState("1", It.Is<WebsocketState>(state =>
                state.Username == "MyUsername" && state.IsLoggedIn
            ))
        );
    }
}