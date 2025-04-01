using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Common;

public class TestClient
{
    private readonly ClientWebSocket _webSocket = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task _receivingTask = Task.CompletedTask;
    private readonly ReplaySubject<LoginResponse> _loginResponses = new();
    private readonly ReplaySubject<ChallengeResponse> _challengeResponse = new();
    private readonly ReplaySubject<MessageResponse> _messageResponse = new();

    public IObservable<MessageResponse> MessageResponses()
    {
        return _messageResponse.AsObservable();
    }

    public async Task Connect(string uri)
    {
        try
        {
            await _webSocket.ConnectAsync(new Uri(uri), _cancellationTokenSource.Token);
            Console.WriteLine("Connected!");
            _receivingTask = ReceiveMessages();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    public async Task<bool> Login(string publicKey, string privateKey, string username, string password)
    {
        await Send(
            new WebsocketMessage<LoginRequest>(MessageType.LoginRequest, new LoginRequest(publicKey, username))
        );

        var loginResponse = await _loginResponses.FirstAsync();
        await HandleLoginResponse(loginResponse, privateKey, password);
        var challengeResponse = await _challengeResponse.FirstAsync();
        return challengeResponse.Success;
    }

    public async Task Send(object content)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            var json = JsonConvert.SerializeObject(content);
            var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
            await _webSocket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
        }
        else
        {
            Console.WriteLine("WebSocket is not connected.");
        }
    }

    public async Task Disconnect()
    {
        if (_webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
            await _cancellationTokenSource.CancelAsync();
            Console.WriteLine("Disconnected!");
        }

        await _receivingTask;
        _receivingTask.Dispose();

        _webSocket.Dispose();
        _cancellationTokenSource.Dispose();
    }

    private async Task ReceiveMessages()
    {
        var buffer = new byte[262144];
        try
        {
            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(buffer, _cancellationTokenSource.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        string.Empty,
                        _cancellationTokenSource.Token
                    );
                    Console.WriteLine("Connection closed by the server");
                }
                else
                {
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    HandleMessage(receivedMessage);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Receive operation was canceled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Receive exception: {ex.Message}");
        }
    }

    private void HandleMessage(string json)
    {
        var jsonObject = JObject.Parse(json);
        var typeString = jsonObject["MessageType"]?.ToString();

        if (!Enum.TryParse(typeString, true, out MessageType messageType))
        {
            throw new ArgumentException($"Unknown type {messageType}");
        }

        switch (messageType)
        {
            case MessageType.LoginResponse:
                _loginResponses.OnNext(JsonConvert.DeserializeObject<WebsocketMessage<LoginResponse>>(json)!.Data);
                break;
            case MessageType.ChallengeResponse:
                _challengeResponse.OnNext(
                    JsonConvert.DeserializeObject<WebsocketMessage<ChallengeResponse>>(json)!.Data);
                break;
            case MessageType.MessageResponse:
                _messageResponse.OnNext(JsonConvert.DeserializeObject<WebsocketMessage<MessageResponse>>(json)!.Data);
                break;
            default:
                throw new ArgumentException($"Unhandled type {messageType}");
        }
    }

    async Task HandleLoginResponse(LoginResponse loginResponse, string privateKey, string password)
    {
        Console.WriteLine("Received Challenge " + loginResponse.ChallangeToken);
        var signedChallenge = Encryption.EncryptMessage(
            loginResponse.ChallangeToken,
            Encryption.ReadPrivateKey(privateKey, password),
            Encryption.ReadPublicKey(loginResponse.SystemPublic)
        );

        await Send(
            new WebsocketMessage<ChallengeRequest>(MessageType.ChallengeRequest, new ChallengeRequest(signedChallenge))
        );
    }
}