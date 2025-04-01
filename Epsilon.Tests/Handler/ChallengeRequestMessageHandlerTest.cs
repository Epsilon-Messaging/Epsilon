using System.Reactive.Subjects;
using AutoFixture;
using Common;
using Common.Models;
using Epsilon.Handler.WebsocketMessageHandler;
using Epsilon.Services.WebsocketStateService;
using FluentAssertions.Reactive;
using Moq;
using Xunit;

namespace Epsilon.Tests.Handler;

public class ChallengeRequestMessageHandlerTest
{
    private readonly Mock<IWebsocketStateService> _websocketStateService = new();
    private readonly ChallengeRequestMessageHandler _challengeRequestMessageHandler;
    private readonly Fixture _fixture = new();

    public ChallengeRequestMessageHandlerTest()
    {
        _websocketStateService
            .Setup(service => service.GetWebsocketState(It.IsAny<string>()))
            .Returns(_fixture.Create<WebsocketState>());

        _challengeRequestMessageHandler = new ChallengeRequestMessageHandler(
            _websocketStateService.Object
        );
    }

    [Fact]
    public void ChallengeRequest_ShouldReturnTrue_WhenWeSendTheCorrectEncryptedMessage()
    {
        var replay = new ReplaySubject<object>();
        var sessionId = Guid.NewGuid().ToString();
        var (userPublicKey, userPrivateKey) = Encryption.GenerateKeys("Password123", "testUser");
        var (systemPublicKey, systemPrivateKey) = Encryption.GenerateKeys(sessionId, "system");
        var challengeToken = Guid.NewGuid();

        _websocketStateService
            .Setup(service => service.GetWebsocketState(It.IsAny<string>()))
            .Returns(new WebsocketState
            {
                ChallangeToken = challengeToken,
                PublicKey = userPublicKey,
                SystemPrivateKey = systemPrivateKey,
                OutgoingMessages = replay
            });

        var encryptedMessage = Encryption.EncryptMessage(
            challengeToken.ToString(),
            Encryption.ReadPrivateKey(userPrivateKey, "Password123"),
            Encryption.ReadPublicKey(systemPublicKey)
        );

        _challengeRequestMessageHandler.HandleMessage(new ChallengeRequest
        {
            SignedChallenge = encryptedMessage
        }, sessionId);

        _websocketStateService.Verify(service =>
            service.SetWebsocketState(sessionId, new WebsocketState
            {
                ChallangeToken = challengeToken,
                PublicKey = userPublicKey,
                SystemPrivateKey = systemPrivateKey,
                IsLoggedIn = true,
                OutgoingMessages = replay
            })
        );

        replay.Observe().Should().PushMatchAsync(o => o.Equals(new ChallengeResponse
        {
            Success = true
        }));
    }

    [Fact]
    public void ChallengeRequest_ShouldReturnTrue_WhenWeSendTheWrongEncryptedMessage()
    {
        var replay = new ReplaySubject<object>();
        var sessionId = Guid.NewGuid().ToString();
        var (userPublicKey, _) = Encryption.GenerateKeys("Password123", "testUser");
        var (_, userPrivateKey) = Encryption.GenerateKeys("Password123", "testUser");
        var (systemPublicKey, systemPrivateKey) = Encryption.GenerateKeys(sessionId, "system");
        var challengeToken = Guid.NewGuid();

        _websocketStateService
            .Setup(service => service.GetWebsocketState(It.IsAny<string>()))
            .Returns(new WebsocketState
            {
                ChallangeToken = challengeToken,
                PublicKey = userPublicKey,
                SystemPrivateKey = systemPrivateKey,
                OutgoingMessages = replay
            });

        var encryptedMessage = Encryption.EncryptMessage(
            challengeToken.ToString(),
            Encryption.ReadPrivateKey(userPrivateKey, "Password123"),
            Encryption.ReadPublicKey(systemPublicKey)
        );

        _challengeRequestMessageHandler.HandleMessage(new ChallengeRequest
        {
            SignedChallenge = encryptedMessage
        }, sessionId);

        _websocketStateService.Verify(service =>
                service.SetWebsocketState(It.IsAny<string>(), It.IsAny<WebsocketState>()), Times.Never
        );

        replay.Observe().Should().PushMatchAsync(o => o.Equals(new ChallengeResponse
        {
            Success = false
        }));
    }
}