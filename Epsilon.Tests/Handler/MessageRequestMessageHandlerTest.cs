using System.Reactive.Linq;
using AutoFixture;
using Common.Models;
using Epsilon.Handler.WebsocketMessageHandler;
using Epsilon.Services.WebsocketStateService;
using FluentAssertions;
using FluentAssertions.Reactive;
using Moq;
using Xunit;

namespace Epsilon.Tests.Handler;

public class MessageRequestMessageHandlerTest
{
    private readonly Mock<IWebsocketStateService> _websocketStateService = new();
    private readonly MessageRequestMessageHandler _messageRequestMessageHandler;
    private readonly List<WebsocketState> _websocketStates;
    private readonly Fixture _fixture = new();

    public MessageRequestMessageHandlerTest()
    {
        _websocketStates = _fixture.CreateMany<WebsocketState>().ToList();

        _websocketStateService
            .Setup(service => service.GetWebsocketState(It.IsAny<string>()))
            .Returns(_fixture.Create<WebsocketState>() with
            {
                IsLoggedIn = true
            });

        _websocketStateService
            .Setup(service => service.GetAllActiveWebsockets())
            .Returns(_websocketStates);

        _messageRequestMessageHandler = new MessageRequestMessageHandler(
            _websocketStateService.Object
        );
    }

    [Fact]
    public void MessageRequestMessageHandler_ShouldDoNothing_WhenMessageRequestIsNull()
    {
        var sessionId = Guid.NewGuid().ToString();

        var action = () => _messageRequestMessageHandler.HandleMessage(null, sessionId);

        action.Should().NotThrow();
    }

    [Fact]
    public void MessageRequestMessageHandler_ShouldDoNothing_WhenWeAreNotLoggedIn()
    {
        const string recipient = "Recipient";
        var messageRequest = new MessageRequest("Hello :)", recipient);
        var sessionId = Guid.NewGuid().ToString();
        _websocketStates.Add(_fixture.Create<WebsocketState>() with
        {
            Username = recipient,
            IsLoggedIn = true
        });

        _websocketStateService
            .Setup(service => service.GetWebsocketState(It.IsAny<string>()))
            .Returns(_fixture.Create<WebsocketState>() with
            {
                IsLoggedIn = false,
                Username = "MyUsername"
            });

        _messageRequestMessageHandler.HandleMessage(messageRequest, sessionId);

        foreach (var websocketState in _websocketStates)
        {
            websocketState.OutgoingMessages.AsObservable().Observe().Should().NotPush();
        }
    }

    [Fact]
    public void MessageRequestMessageHandler_ShouldSendMessage_WhenWeAreLoggedIn()
    {
        const string publicKey = "PublicKey";
        const string username = "MyUsername";
        var messageRequest = new MessageRequest("Hello :)", publicKey);
        var sessionId = Guid.NewGuid().ToString();
        _websocketStates.Add(_fixture.Create<WebsocketState>() with
        {
            Username = username,
            PublicKey = publicKey,
            IsLoggedIn = true
        });

        _websocketStateService
            .Setup(service => service.GetWebsocketState(It.IsAny<string>()))
            .Returns(_fixture.Create<WebsocketState>() with
            {
                IsLoggedIn = true,
                Username = username,
                PublicKey = publicKey
            });

        _messageRequestMessageHandler.HandleMessage(messageRequest, sessionId);

        foreach (var websocketState in _websocketStates)
        {
            if (websocketState.PublicKey == publicKey)
            {
                websocketState.OutgoingMessages.AsObservable().Observe().Should()
                    .PushMatch(s => s.Equals(new WebsocketMessage<MessageResponse>(
                        MessageType.MessageResponse,
                        new MessageResponse("Hello :)", publicKey, username))
                    ));
            }
            else
            {
                websocketState.OutgoingMessages.AsObservable().Observe().Should().NotPush();
            }
        }
    }
}