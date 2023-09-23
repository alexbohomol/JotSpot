using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Identity.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Tests;

public class AuthenticationTests : IntegrationTest
{
    private const string GetTokenUrl = "api/auth/token";
    private const string Login = "alex@jotspot.com";

    [Fact]
    public async Task GetToken_ReturnsUnauthorized()
    {
        // Act
        var msg = await SutClient.PostAsJsonAsync(GetTokenUrl, new TokenRequest(Login, "123"));

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
        var msg = await SutClient.PostAsJsonAsync(GetTokenUrl, new TokenRequest(Login, "1234567"));

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.OK);
        msg.Content.Should().NotBeNull();

        var response = await msg.Content.ReadAsStringAsync();
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = tokenHandler.ReadJwtToken(response);
        
        var nbf = ((DateTimeOffset) jwtSecurityToken.ValidFrom).ToUnixTimeSeconds();
        var exp = ((DateTimeOffset) jwtSecurityToken.ValidTo).ToUnixTimeSeconds();
        (exp - nbf).Should().Be(3600); // 1 hour difference

        jwtSecurityToken.Payload.Should().HaveCount(9);
        jwtSecurityToken.Claims.Should().HaveCount(9);
        jwtSecurityToken.Claims.Select(x => new
        {
            x.Type,
            x.Value
        }).Should().BeEquivalentTo(new[]
        {
            new { Type = "sub", Value = "1" },
            new { Type = "login", Value = Login },
            new { Type = "given_name", Value = "Alex" },
            new { Type = "family_name", Value = "Bohomol" },
            new { Type = "city", Value = "Kyiv" },
            new { Type = "nbf", Value = nbf.ToString() },
            new { Type = "exp", Value = exp.ToString() },
            new { Type = "iss", Value = "https://localhost:7145" },
            new { Type = "aud", Value = "jotspotapi" }
        });
    }
}