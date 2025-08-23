using API_BITLIBRO.Models;

namespace API_BITLIBRO.Interfaces.Repositories;

public interface IAuthRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(string id);
    Task<IList<string>> GetRolesAsync(User user);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task SignOutAsync();
}
