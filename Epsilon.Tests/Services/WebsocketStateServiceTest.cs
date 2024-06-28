using System.Reactive.Subjects;
using Epsilon.Models;
using Epsilon.Services.WebsocketStateService;
using FluentAssertions;
using Xunit;

namespace Epsilon.Tests.Services;

public class WebsocketStateServiceTest
{
    private readonly WebsocketStateService _websocketStateService = new();

    [Fact]
    public void WebsocketStateService_ShouldCreateWebsocket_WhenSessionIdIsProvided()
    {
        var sessionId = Guid.NewGuid().ToString();

        _websocketStateService.CreateWebsocket(sessionId);
        var websocketState = _websocketStateService.GetWebsocketState(sessionId);

        websocketState.Should().BeOfType<WebsocketState>();
        websocketState.IsLoggedIn.Should().BeFalse();
        websocketState.Username.Should().BeEmpty();
    }

    [Fact]
    public void WebsocketStateService_ShouldThrowException_WhenSessionIdIsNotFound()
    {
        var sessionId = Guid.NewGuid().ToString();

        _websocketStateService.CreateWebsocket(sessionId);

        _websocketStateService.GetWebsocketState(sessionId).Should().BeOfType<WebsocketState>();

        _websocketStateService.DeleteWebsocket(sessionId);

        Assert.Throws<ArgumentException>(() => _websocketStateService.GetWebsocketState(sessionId));
    }

    [Fact]
    public void WebsocketStateService_ShouldReturnAllActiveWebsockets_WhenCalled()
    {
        const int count = 10;
        for (var i = 0; i < count; i++)
        {
            _websocketStateService.CreateWebsocket(Guid.NewGuid().ToString());
        }

        _websocketStateService.GetAllActiveWebsockets().Should().HaveCount(count);
    }

    [Fact]
    public void WebsocketStateService_ShouldUpdateWebsocketState_WhenSessionIdIsProvided()
    {
        var sessionId = Guid.NewGuid().ToString();
        _websocketStateService.CreateWebsocket(sessionId);
        var newWebsocketState = new WebsocketState(
            "newState",
            true,
            new ReplaySubject<object>()
        );

        _websocketStateService.SetWebsocketState(sessionId, newWebsocketState);
        _websocketStateService.GetWebsocketState(sessionId).Should().Be(newWebsocketState);
    }
}