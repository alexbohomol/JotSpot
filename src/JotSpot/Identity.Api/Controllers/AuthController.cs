using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Controllers;

[Route("api/{controller}")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    [HttpPost("token")]
    public ActionResult<string> GetToken(TokenRequest request)
    {
        var user = ValidateUserCredentials(
            request.Login,
            request.Password);

        if (user is null)
        {
            return Unauthorized();
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(
                _configuration["Authentication:SecretKey"] 
                ?? throw new Exception("SecretKey is missing in config")));

        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var userClaims = new []
        {
            new Claim("sub", user.UserId.ToString()),
            new Claim("login", user.Login),
            new Claim("given_name", user.FirstName),
            new Claim("family_name", user.LastName),
            new Claim("city", user.City),
        };

        var issuedAt = DateTime.UtcNow;
        
        var jwtSecurityToken = new JwtSecurityToken(
            _configuration["Authentication:Issuer"],
            _configuration["Authentication:Audience"],
            userClaims,
            issuedAt,
            issuedAt.AddHours(1),
            signingCredentials);
        
        var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        
        return Ok(tokenToReturn);
    }

    // grab user from storage or API, but mock for now
    private User? ValidateUserCredentials(string? userName, string? password) =>
        userName == "alex@jotspot.com" && password == "1234567"
            ? new(1, userName, "Alex", "Bohomol", "Kyiv")
            : null;
}
