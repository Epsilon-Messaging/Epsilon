using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Epsilon.Models;
using FluentAssertions.Reactive;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntegrationTests;

public class WebSocketTestClient
{
    private readonly ClientWebSocket _webSocket = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task _receivingTask = Task.CompletedTask;
    private readonly ReplaySubject<object> _recievedMessages = new();

    public FluentTestObserver<object> ReceivedMessages()
    {
        return _recievedMessages.AsObservable().Observe();
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

    public async Task Send(object content)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            var json = JsonConvert.SerializeObject(content);
            var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
            await _webSocket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
            Console.WriteLine($"Sent: {content}");
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
        var buffer = new byte[1024];
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
                    var message = ConvertMessage(receivedMessage);
                    if (message != null) _recievedMessages.OnNext(message);
                    Console.WriteLine($"Received: {receivedMessage}");
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

    private static object? ConvertMessage(string json)
    {
        var jsonObject = JObject.Parse(json);
        var typeString = jsonObject["MessageType"]?.ToString();

        if (!Enum.TryParse(typeString, true, out MessageType messageType))
        {
            throw new ArgumentException($"Unknown type {messageType}");
        }

        return messageType switch
        {
            MessageType.LoginRequest => JsonConvert.DeserializeObject<WebsocketMessage<LoginRequest>>(json),
            MessageType.LoginResponse => JsonConvert.DeserializeObject<WebsocketMessage<LoginResponse>>(json),
            MessageType.MessageResponse => JsonConvert.DeserializeObject<WebsocketMessage<MessageResponse>>(json),
            _ => null
        };
    }
}