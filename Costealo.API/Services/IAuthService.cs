using Costealo.API.Models;

namespace Costealo.API.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(User user, string password);
    Task<string?> LoginAsync(string username, string password);
}
