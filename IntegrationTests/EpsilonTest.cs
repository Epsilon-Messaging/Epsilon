using Common;
using Common.Models;
using FluentAssertions.Reactive;
using Xunit;

namespace IntegrationTests;

public class EpsilonTest
{
    [Fact]
    public async Task Epsilon_ShouldNotSendMessageToUser_WhenWeHaveNotCompletedLogin()
    {
        var (publicKey1, privateKey1) = Encryption.GenerateKeys("Password1", "testuser1");
        var (publicKey2, _) = Encryption.GenerateKeys("Password2", "testuser2");

        var testClient1 = new TestClient();
        var testClient2 = new TestClient();

        await testClient1.Connect("ws://localhost:5172/api/websocket");
        await testClient2.Connect("ws://localhost:5172/api/websocket");

        await testClient1.Login(publicKey1, privateKey1, "User1", "Password1");

        await testClient2.Send(
            new WebsocketMessage<LoginRequest>(MessageType.LoginRequest, new LoginRequest(publicKey2, "User2"))
        );

        await testClient1.Send(
            new WebsocketMessage<MessageRequest>(MessageType.MessageRequest, new MessageRequest("Hello :)", publicKey2))
        );

        await Task.Delay(1000);

        testClient2.MessageResponses().Observe()
            .Should()
            .NotPush();

        await testClient1.Disconnect();
        await testClient2.Disconnect();
    }

    [Fact]
    public async Task Epsilon_ShouldSendMessageToUser_WhenWeAreLoggedOn()
    {
        var (publicKey1, privateKey1) = Encryption.GenerateKeys("Password1", "testuser1");
        var (publicKey2, privateKey2) = Encryption.GenerateKeys("Password2", "testuser2");

        var testClient1 = new TestClient();
        var testClient2 = new TestClient();

        await testClient1.Connect("ws://localhost:5172/api/websocket");
        await testClient2.Connect("ws://localhost:5172/api/websocket");

        await testClient1.Login(publicKey1, privateKey1, "User1", "Password1");
        await testClient2.Login(publicKey2, privateKey2, "User2", "Password2");

        await testClient1.Send(
            new WebsocketMessage<MessageRequest>(MessageType.MessageRequest, new MessageRequest("Hello :)", publicKey2))
        );

        await testClient2.MessageResponses().Observe()
            .Should()
            .PushMatchAsync(response => response.Equals(new MessageResponse
            {
                Message = "Hello :)",
                Username = "User1",
                PublicKey = publicKey1
            }));

        await testClient1.Disconnect();
        await testClient2.Disconnect();
    }

    [Fact]
    public async Task Epsilon_ShouldNotSendMessageToUser_WhenUsernamesDontMatch()
    {
        var (publicKey1, privateKey1) = Encryption.GenerateKeys("Password1", "testuser1");
        var (publicKey2, privateKey2) = Encryption.GenerateKeys("Password2", "testuser2");
        var (publicKey3, privateKey3) = Encryption.GenerateKeys("Password3", "testuser2");

        var testClient1 = new TestClient();
        var testClient2 = new TestClient();
        var testClient3 = new TestClient();

        await testClient1.Connect("ws://localhost:5172/api/websocket");
        await testClient2.Connect("ws://localhost:5172/api/websocket");
        await testClient3.Connect("ws://localhost:5172/api/websocket");

        await testClient1.Login(publicKey1, privateKey1, "User1", "Password1");
        await testClient2.Login(publicKey2, privateKey2, "User2", "Password2");
        await testClient3.Login(publicKey3, privateKey3, "User3", "Password3");

        await testClient1.Send(
            new WebsocketMessage<MessageRequest>(MessageType.MessageRequest, new MessageRequest("Hello :)", publicKey3))
        );

        await Task.Delay(1000);

        testClient2.MessageResponses().Observe()
            .Should()
            .NotPush();

        await testClient3.MessageResponses().Observe()
            .Should()
            .PushMatchAsync(response => response.Equals(new MessageResponse
            {
                Message = "Hello :)",
                Username = "User1",
                PublicKey = publicKey1
            }));

        await testClient1.Disconnect();
        await testClient2.Disconnect();
        await testClient3.Disconnect();
    }
}