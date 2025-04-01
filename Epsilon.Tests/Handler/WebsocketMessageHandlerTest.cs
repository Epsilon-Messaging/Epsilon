using AutoFixture;
using Common.Models;
using Epsilon.Handler.WebsocketMessageHandler;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Epsilon.Tests.Handler;

public class WebsocketMessageHandlerTest
{
    private readonly Mock<IMessageHandler<LoginRequest>> _loginRequestHandler = new();
    private readonly Mock<IMessageHandler<ChallengeRequest>> _challengeRequestHandler = new();
    private readonly Mock<IMessageHandler<MessageRequest>> _messageRequestHandler = new();

    private readonly WebsocketMessageHandler _websocketMessageHandler;
    private readonly Fixture _fixture = new();

    public WebsocketMessageHandlerTest()
    {
        _websocketMessageHandler = new WebsocketMessageHandler(
            _loginRequestHandler.Object,
            _challengeRequestHandler.Object,
            _messageRequestHandler.Object
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("{\"MessageType\":\"Invalid\"}")]
    [InlineData("{\"MessageType\":\"Login\", \"Data\": null}")]
    [InlineData("{\"MessageType\":\"LoginResponse\", \"Data\": null}")]
    public void WebsocketMessageHandler_ShouldDoNothing_WhenWeSendAnInvalidMessage(string? message)
    {
        var sessionId = Guid.NewGuid().ToString();

        var action = () => _websocketMessageHandler.HandleMessage(message, sessionId);

        action.Should().NotThrow();
    }

    [Fact]
    public void WebsocketMessageHandler_ShouldCallLoginRequestHandler_WhenWeSendALoginRequest()
    {
        var loginRequest = _fixture.Create<WebsocketMessage<LoginRequest>>() with
        {
            MessageType = MessageType.LoginRequest
        };
        var sessionId = Guid.NewGuid().ToString();

        _websocketMessageHandler.HandleMessage(JsonConvert.SerializeObject(loginRequest), sessionId);

        _loginRequestHandler.Verify(service =>
            service.HandleMessage(loginRequest.Data, sessionId)
        );
    }

    [Fact]
    public void WebsocketMessageHandler_ShouldCallMessageRequestHandler_WhenWeSendAMessageRequest()
    {
        var loginRequest = _fixture.Create<WebsocketMessage<MessageRequest>>() with
        {
            MessageType = MessageType.MessageRequest
        };
        var sessionId = Guid.NewGuid().ToString();

        _websocketMessageHandler.HandleMessage(JsonConvert.SerializeObject(loginRequest), sessionId);

        _messageRequestHandler.Verify(service =>
            service.HandleMessage(loginRequest.Data, sessionId)
        );
    }

    [Fact]
    public void WebsocketMessageHandler_ShouldCallChallengeRequestHandler_WhenWeSendAChallengeRequest()
    {
        var challengeRequest = _fixture.Create<WebsocketMessage<ChallengeRequest>>() with
        {
            MessageType = MessageType.ChallengeRequest
        };
        var sessionId = Guid.NewGuid().ToString();

        _websocketMessageHandler.HandleMessage(JsonConvert.SerializeObject(challengeRequest), sessionId);

        _challengeRequestHandler.Verify(service =>
            service.HandleMessage(challengeRequest.Data, sessionId)
        );
    }
}