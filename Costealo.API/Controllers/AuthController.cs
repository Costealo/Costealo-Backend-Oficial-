using Costealo.API.Data;
using Costealo.API.Models;
using Costealo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Costealo.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ApplicationDbContext _context;

    public AuthController(IAuthService authService, ApplicationDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    // POST: api/Auth/register-first-admin
    [HttpPost("register-first-admin")]
    public async Task<ActionResult<User>> RegisterFirstAdmin(AdminRegistrationDto request)
    {
        // Check if any admin already exists
        if (await _context.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            return Forbid("An admin already exists. Use the protected endpoint to create additional admins.");
        }

        var admin = new User
        {
            Name = request.Name,
            Email = request.Email,
            Role = UserRole.Admin
        };

        var result = await _authService.RegisterAsync(admin, request.Password);
        if (result == null)
        {
            return BadRequest("Admin already exists.");
        }

        return Ok(result);
    }

    // POST: api/Auth/register-admin
    [HttpPost("register-admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<User>> RegisterAdmin(AdminRegistrationDto request)
    {
        var admin = new User
        {
            Name = request.Name,
            Email = request.Email,
            Role = UserRole.Admin
        };

        var result = await _authService.RegisterAsync(admin, request.Password);
        if (result == null)
        {
            return BadRequest("Admin already exists.");
        }

        return Ok(result);
    }

    // POST: api/Auth/login
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginDto request)
    {
        var token = await _authService.LoginAsync(request.Email, request.Password);
        if (token == null)
        {
            return BadRequest("Invalid email or password.");
        }

        return Ok(token);
    }
}

public class AdminRegistrationDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
