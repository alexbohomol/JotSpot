using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using FluentAssertions.Extensions;
using Identity.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Tests;

public class AuthenticationTests : IntegrationTest
{
    private const string GetTokenUrl = "api/auth/token";

    [Fact]
    public async Task GetToken_ReturnsUnauthorized()
    {
        // Act
        var msg = await SutClient.PostAsJsonAsync(
            GetTokenUrl, 
            new TokenRequest("alex@jotspot.com", "123"));

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        msg.Content.Should().NotBeNull();

        var response = await msg.Content.ReadFromJsonAsync<ProblemDetails>();
        response.Should().NotBeNull();
        response!.Type.Should().Be("https://tools.ietf.org/html/rfc7235#section-3.1");
        response.Title.Should().Be(HttpStatusCode.Unauthorized.ToString());
        response.Status.Should().Be((int?)HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task GetToken_ReturnsOk()
    {
        // Act
        var msg = await SutClient.PostAsJsonAsync(
            GetTokenUrl, 
            new TokenRequest("alex@jotspot.com", "1234567"));

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.OK);
        msg.Content.Should().NotBeNull();

        var response = await msg.Content.ReadAsStringAsync();
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = tokenHandler.ReadJwtToken(response);

        jwtSecurityToken.Payload.Should().HaveCount(9);
        jwtSecurityToken.ValidFrom.Should().BeCloseTo(DateTime.UtcNow, 1.Seconds());
        jwtSecurityToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), 1.Seconds());

        var claims = jwtSecurityToken.Claims.ToArray();
        claims.Should().HaveCount(9);
        claims.Should().ContainSingle(x => x.Type == "sub" && x.Value == "1");
        claims.Should().ContainSingle(x => x.Type == "login" && x.Value == "alex@jotspot.com");
        claims.Should().ContainSingle(x => x.Type == "given_name" && x.Value == "Alex");
        claims.Should().ContainSingle(x => x.Type == "family_name" && x.Value == "Bohomol");
        claims.Should().ContainSingle(x => x.Type == "city" && x.Value == "Kyiv");
        claims.Should().ContainSingle(x => x.Type == "nbf");
        claims.Should().ContainSingle(x => x.Type == "exp");
        claims.Should().ContainSingle(x => x.Type == "iss" && x.Value == "https://localhost:7145");
        claims.Should().ContainSingle(x => x.Type == "aud" && x.Value == "jotspotapi");
    }
}