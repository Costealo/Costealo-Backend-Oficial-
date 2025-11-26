using Costealo.API.Data;
using Costealo.API.Models;
using Costealo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Costealo.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;

    public UsersController(ApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    // GET: api/Users
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users
            .Where(u => u.Role != UserRole.Admin)
            .ToListAsync();
    }

    // GET: api/Users/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null || user.Role == UserRole.Admin)
        {
            return NotFound();
        }

        return user;
    }

    // POST: api/Users
    [HttpPost]
    public async Task<ActionResult<User>> Register(UserRegistrationDto request)
    {
        // Only allow Empresa and Independiente registration
        if (request.Role == UserRole.Admin)
        {
            return BadRequest("Cannot register as Admin through this endpoint.");
        }

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

        return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
    }

    // PUT: api/Users/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(int id, UserUpdateDto request)
    {
        var user = await _context.Users.FindAsync(id);
        
        if (user == null || user.Role == UserRole.Admin)
        {
            return NotFound();
        }

        user.Name = request.Name;
        user.Email = request.Email;
        
        // Don't allow role change to Admin
        if (request.Role != UserRole.Admin)
        {
            user.Role = request.Role;
        }

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}

public class UserRegistrationDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

public class UserUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
