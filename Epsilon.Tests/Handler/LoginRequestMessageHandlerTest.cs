using AutoFixture;
using Common.Models;
using Epsilon.Handler.WebsocketMessageHandler;
using Epsilon.Services.WebsocketStateService;
using FluentAssertions;
using Moq;
using Xunit;

namespace Epsilon.Tests.Handler;

public class LoginRequestMessageHandlerTest
{
    private readonly Mock<IWebsocketStateService> _websocketStateService = new();
    private readonly LoginRequestMessageHandler _loginRequestMessageHandler;
    private readonly Fixture _fixture = new();

    public LoginRequestMessageHandlerTest()
    {
        _websocketStateService
            .Setup(service => service.GetWebsocketState(It.IsAny<string>()))
            .Returns(_fixture.Create<WebsocketState>());

        _loginRequestMessageHandler = new LoginRequestMessageHandler(
            _websocketStateService.Object
        );
    }

    [Fact]
    public void LoginRequestMessageHandler_ShouldDoNothing_WhenLoginRequestIsNull()
    {
        var sessionId = Guid.NewGuid().ToString();

        var action = () => _loginRequestMessageHandler.HandleMessage(null, sessionId);

        action.Should().NotThrow();
    }

    [Fact]
    public void LoginRequestMessageHandler_ShouldSetUsername_WhenWeSendALoginRequest()
    {
        var loginRequest = _fixture.Create<LoginRequest>();
        var sessionId = Guid.NewGuid().ToString();

        _loginRequestMessageHandler.HandleMessage(loginRequest, sessionId);

        _websocketStateService.Verify(service =>
            service.SetWebsocketState(sessionId, It.Is<WebsocketState>(state =>
                state.Username == loginRequest.Username && state.IsLoggedIn
            ))
        );
    }
}