using Costealo.API.Models;
using Costealo.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Costealo.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(UserDto request)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Role = request.Role
        };

        var result = await _authService.RegisterAsync(user, request.Password);
        if (result == null)
        {
            return BadRequest("User already exists.");
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(UserLoginDto request)
    {
        var token = await _authService.LoginAsync(request.Email, request.Password);
        if (token == null)
        {
            return BadRequest("Invalid email or password.");
        }

        return Ok(token);
    }
}

public class UserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

public class UserLoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
