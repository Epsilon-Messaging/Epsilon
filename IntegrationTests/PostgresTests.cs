using System.Net;
using Epsilon.Models;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace IntegrationTests;

public class PostgresTests
{
    private readonly HttpClient _client = new();

    [Fact]
    public async Task Epsilon_ShouldReturnListOfUsers()
    {
        var response = await _client.GetAsync("http://localhost:5172/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var users = JsonConvert.DeserializeObject<List<User>>(body)!;
        users.Should().HaveCount(4);
    }
}