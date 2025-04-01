using Common;
using Common.Models;

var client = new TestClient();

var (publicKey, privateKey) = Encryption.GenerateKeys("Password123", "testuser");

client.MessageResponses().Subscribe(HandleMessageResponse);

await client.Connect("ws://localhost:5172/api/websocket");


Console.WriteLine("What is your username?");
var username = Console.ReadLine() ?? "";

var success = await client.Login(publicKey, privateKey, username, "Password123");

Console.WriteLine(success ? "Login Successful" : "Login Failed");

string? user = null;

while (true)
{
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

void HandleMessageResponse(MessageResponse messageResponse)
{
    var decryptedMessage = Encryption.DecryptMessage(
        messageResponse.Message,
        Encryption.ReadPrivateKey(privateKey, "Password123"),
        Encryption.ReadPublicKey(messageResponse.PublicKey)
    );

    Console.WriteLine(messageResponse.Username + ": " + decryptedMessage);
}