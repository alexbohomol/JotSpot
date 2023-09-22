using System.Net.Http.Json;
using Identity.Api.Controllers;

namespace Identity.Api.Tests;

public class AuthenticationTests : IntegrationTest
{
    [Fact]
    public async Task GetToken_ReturnsUnauthorized()
    {
        // Act
        var msg = await SutClient.PostAsJsonAsync("api/auth/token", new TokenRequest("alex@jotspot.com", "123"));

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        msg.Content.Should().NotBeNull();
        var body = await msg.Content.ReadAsStringAsync();
        body.Should().ContainAll(
            "https://tools.ietf.org/html/rfc7235#section-3.1", 
            "401", 
            "Unauthorized");
    }
    
    [Fact]
    public async Task GetToken_ReturnsOk()
    {
        // Act
        var msg = await SutClient.PostAsJsonAsync("api/auth/token", new TokenRequest("alex@jotspot.com", "1234567"));

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.OK);
        msg.Content.Should().NotBeNull();
        // TODO: refine to assert JWT params properly
    }
}