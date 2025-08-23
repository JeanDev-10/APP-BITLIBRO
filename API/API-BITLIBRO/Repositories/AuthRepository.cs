using API_BITLIBRO.Interfaces.Repositories;
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Identity;

namespace API_BITLIBRO.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthRepository(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public Task<User?> FindByEmailAsync(string email) =>
        _userManager.FindByEmailAsync(email)!;

    public Task<User?> FindByIdAsync(string id) =>
        _userManager.FindByIdAsync(id)!;

    public async Task<IList<string>> GetRolesAsync(User user) =>
        await _userManager.GetRolesAsync(user);

    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        return result.Succeeded;
    }

    public Task SignOutAsync() =>
        _signInManager.SignOutAsync();
}
