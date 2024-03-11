using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using FluentValidation;

using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapPost("api/auth/token", GetToken);

app.Run();

static IResult GetToken(TokenRequest request, IConfiguration configuration)
{
    var validator = new TokenRequestValidator();
    var validationResult = validator.Validate(request);
    if (validationResult.Errors.Any())
    {
        return Results.BadRequest();
    }

    var user = ValidateUserCredentials(
        request.Login,
        request.Password);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    var securityKey = new SymmetricSecurityKey(
        Encoding.ASCII.GetBytes(
            configuration["Authentication:SecretKey"]
            ?? throw new Exception("SecretKey is missing in config")));

    var signingCredentials = new SigningCredentials(
        securityKey,
        SecurityAlgorithms.HmacSha256);

    var userClaims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
        new Claim("login", user.Login),
        new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
        new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
        new Claim(JwtRegisteredClaimNames.Email, user.Email)
    };

    var issuedAt = DateTime.UtcNow;

    var jwtSecurityToken = new JwtSecurityToken(
        configuration["Authentication:Issuer"],
        configuration["Authentication:Audience"],
        userClaims,
        issuedAt,
        issuedAt.AddHours(1),
        signingCredentials);

    var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

    return Results.Text(tokenToReturn);
}

// grab user from storage or API, but mock for now
static User? ValidateUserCredentials(string? userName, string? password) =>
    userName == "alex" && password == "1234567"
        ? new(1, userName, "alex@jotspot.com", "Alex", "Bohomol")
        : null;

public record TokenRequest(string? Login, string? Password);

internal class TokenRequestValidator : AbstractValidator<TokenRequest>
{
    public TokenRequestValidator()
    {
        RuleFor(x => x.Login).NotEmpty().Length(4, 20).Matches("^[a-zA-Z0-9]*$");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(7);
    }
}

record User(int UserId, string Login, string Email, string FirstName, string LastName);
