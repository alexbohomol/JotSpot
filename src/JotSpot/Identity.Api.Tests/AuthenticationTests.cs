using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;

namespace Identity.Api.Tests;

public class AuthenticationTests : IntegrationTest
{
    private const string GetTokenUrl = "api/auth/token";
    private const string Login = "alex";

    [Theory]
    [InlineData("")]
    [InlineData("u")]
    [InlineData("us")]
    [InlineData("usr")]
    [InlineData("use@")]
    [InlineData("UserUserUserUserUser1")]
    public async Task GetToken_InvalidUserName_ReturnsBadRequest(string userName)
    {
        // Act
        var msg = await SutClient.PostAsJsonAsync(GetTokenUrl, new TokenRequest(userName, "123"));

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        msg.Content.Should().NotBeNull();
        var response = await msg.Content.ReadAsStringAsync();
        response.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("p")]
    [InlineData("pa")]
    [InlineData("pas")]
    [InlineData("pass")]
    [InlineData("passw")]
    [InlineData("passwo")]
    public async Task GetToken_InvalidPassword_ReturnsBadRequest(string password)
    {
        // Act
        var msg = await SutClient.PostAsJsonAsync(GetTokenUrl, new TokenRequest(Login, password));

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        msg.Content.Should().NotBeNull();
        var response = await msg.Content.ReadAsStringAsync();
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task GetToken_ReturnsUnauthorized()
    {
        // Act
        var msg = await SutClient.PostAsJsonAsync(GetTokenUrl, new TokenRequest(Login, "12345678"));

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        msg.Content.Should().NotBeNull();
        var response = await msg.Content.ReadAsStringAsync();
        response.Should().BeEmpty();
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

        var nbf = ((DateTimeOffset)jwtSecurityToken.ValidFrom).ToUnixTimeSeconds();
        var exp = ((DateTimeOffset)jwtSecurityToken.ValidTo).ToUnixTimeSeconds();
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
            new { Type = "email", Value = "alex@jotspot.com" },
            new { Type = "nbf", Value = nbf.ToString() },
            new { Type = "exp", Value = exp.ToString() },
            new { Type = "iss", Value = "https://localhost:7145" },
            new { Type = "aud", Value = "jotspotapi" }
        });
    }
}
