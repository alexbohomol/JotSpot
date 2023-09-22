using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JotSpot.Api.Controllers;

[Route("api/{controller}")]
[ApiController]
[Authorize]
public class SecretController : ControllerBase
{
    public string[] Get() => new[] { "top", "secret", "files" };
    
    [Route("user")]
    public ClaimsPrincipal GetUser() => User;
}