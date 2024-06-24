using System.Net;
using FluentAssertions;
using Xunit;

namespace IntegrationTests;

public class Epsilon
{
    private readonly HttpClient _client = new();

    [Fact]
    public async Task Epsilon_ShouldReturnHelloWorld()
    {
        var response = await _client.GetAsync("http://localhost:5172/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("Hello World!");
    }
}