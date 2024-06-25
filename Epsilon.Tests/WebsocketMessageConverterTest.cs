using AutoFixture;
using Epsilon.Models;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Epsilon.Tests;

public class WebsocketMessageConverterTest
{
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly Fixture _fixture = new();

    public WebsocketMessageConverterTest()
    {
        _serializerSettings = new JsonSerializerSettings();
        _serializerSettings.Converters.Add(new WebsocketMessageConverter());
    }

    [Fact]
    public void WebsocketMessageConvert_ShouldSerializeAndDeserializeLoginRequests()
    {
        var message = new WebsocketMessage<LoginRequest>(MessageType.LoginRequest, _fixture.Create<LoginRequest>());

        var jsonString = JsonConvert.SerializeObject(message, _serializerSettings);

        JsonConvert.DeserializeObject<WebsocketMessage<LoginRequest>>(jsonString, _serializerSettings)
            .Should()
            .BeEquivalentTo(message);
    }

    [Fact]
    public void WebsocketMessageConvert_ShouldSerializeAndDeserializeLoginResponse()
    {
        var message = new WebsocketMessage<LoginResponse>(MessageType.LoginResponse, _fixture.Create<LoginResponse>());

        var jsonString = JsonConvert.SerializeObject(message, _serializerSettings);

        JsonConvert.DeserializeObject<WebsocketMessage<LoginResponse>>(jsonString, _serializerSettings)
            .Should()
            .BeEquivalentTo(message);
    }

    [Fact]
    public void WebsocketMessageConvert_ShouldSerializeAndDeserializeMessageResponse()
    {
        var message = new WebsocketMessage<MessageResponse>(MessageType.MessageResponse, _fixture.Create<MessageResponse>());

        var jsonString = JsonConvert.SerializeObject(message, _serializerSettings);

        JsonConvert.DeserializeObject<WebsocketMessage<MessageResponse>>(jsonString, _serializerSettings)
            .Should()
            .BeEquivalentTo(message);
    }
}