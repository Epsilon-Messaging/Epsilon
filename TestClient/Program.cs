using System.Reactive.Linq;
using Common;
using Epsilon.Models;

var client = new TestClient.TestClient();

var (publicKey, privateKey) = Encryption.GenerateKeys("Password123", "testuser");

var IsLoggedIn = false;

await client.Connect("ws://localhost:5172/api/websocket");

client.ReceivedMessages().Subscribe(HandleMessages);

Console.WriteLine("What is your username?");

await client.Send(
    new WebsocketMessage<LoginRequest>(MessageType.LoginRequest, new LoginRequest(publicKey, Console.ReadLine() ?? ""))
);

string? user = null;

while (true)
{
    await Task.Delay(100);
    if (!IsLoggedIn) continue;
    if (user == null)
    {
        Console.WriteLine("Enter User you want to send a message to");
        user = Console.ReadLine() ?? "";
    }
    Console.WriteLine("Enter Message");
    var message = Console.ReadLine() ?? "";

    var encryptedMessage = Encryption.EncryptMessage(
        message,
        Encryption.ReadPrivateKey(privateKey, "Password123"),
        Encryption.ReadPublicKey(user)
    );

    await client.Send(
        new WebsocketMessage<MessageRequest>(MessageType.MessageRequest, new MessageRequest(encryptedMessage, user))
    );
}

async void HandleMessages(object message)
{
    if (message is WebsocketMessage<LoginResponse> loginResponse)
    {
        Console.WriteLine("Received Challenge " + loginResponse.Data.ChallangeToken);
        var signedChallenge = Encryption.EncryptMessage(
            loginResponse.Data.ChallangeToken,
            Encryption.ReadPrivateKey(privateKey, "Password123"),
            Encryption.ReadPublicKey(loginResponse.Data.SystemPublic)
        );

        await client.Send(
            new WebsocketMessage<ChallengeRequest>(MessageType.ChallengeRequest, new ChallengeRequest(signedChallenge))
        );
    }

    if (message is WebsocketMessage<ChallengeResponse> challengeResponse)
    {
        if (challengeResponse.Data.Success == false)
        {
            Console.WriteLine("Something went wrong :c");
        }
        else
        {
            Console.WriteLine("");
            Console.WriteLine("Logged in as " + publicKey);

            IsLoggedIn = true;
        }
    }

    if (message is WebsocketMessage<MessageResponse> messageResponse)
    {
        var decryptedMessage = Encryption.DecryptMessage(
            messageResponse.Data.Message,
            Encryption.ReadPrivateKey(privateKey, "Password123"),
            Encryption.ReadPublicKey(messageResponse.Data.PublicKey)
        );

        Console.WriteLine(messageResponse.Data.Username + ": " + decryptedMessage);

    }
}