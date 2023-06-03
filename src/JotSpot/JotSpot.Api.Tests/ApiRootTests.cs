using Microsoft.AspNetCore.Mvc.Testing;

namespace JotSpot.Api.Tests;

public class ApiRootTests
{
    private readonly HttpClient _client;

    public ApiRootTests()
    {
        var factory = new WebApplicationFactory<Program>();
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetRoot_Returns_HelloWorld()
    {
        var msg = await _client.GetAsync("");

        msg.StatusCode.Should().Be(HttpStatusCode.OK);
        msg.Content.Should().NotBeNull();
        var body = await msg.Content.ReadAsStringAsync();
        body.Should().Be("Hello World!");
    }
}