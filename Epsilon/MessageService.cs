using System.Reactive.Subjects;
using Epsilon.Models;
using Epsilon.Services.WebsocketStateService;

namespace Epsilon;

public class MessageService : BackgroundService
{
    public static ReplaySubject<WebsocketMessage<MessageResponse>> OutgoingMessages = new();

    private readonly IWebsocketStateService _websocketStateService;

    public MessageService(IWebsocketStateService websocketStateService)
    {
        _websocketStateService = websocketStateService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var websocketState in _websocketStateService.GetAllActiveWebsockets())
            {
                OutgoingMessages.OnNext(new WebsocketMessage<MessageResponse>(MessageType.MessageResponse,
                    new MessageResponse($"Hello {websocketState.Username}", websocketState.Username)
                ));
            }

            await Task.Delay(100, stoppingToken);
        }
    }
}