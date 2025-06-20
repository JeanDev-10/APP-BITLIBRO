using System;
using API_BITLIBRO.DTOs.Auth.Login;
using API_BITLIBRO.DTOs.Auth.Me;
using API_BITLIBRO.Interfaces;
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Identity;

namespace API_BITLIBRO.Services;

public class AuthService:IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;
    public AuthService(UserManager<User> userManager,
            SignInManager<User> signInManager, ITokenService tokenService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenService = tokenService;
    }
    public async Task<LoginResponseDTO> LoginAsync(LoginDTO loginDTO)
    {
        var user = await _userManager.FindByEmailAsync(loginDTO.Email);

        if (user == null)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);

        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        var token = await _tokenService.GenerateToken(user);
        return new LoginResponseDTO
        {
            Token = token,
            User = new UserInfoDTO
            {
                Id = user.Id,
                Email = user.Email!,
                Name = user.Name,
                LastName = user.LastName,
                Ci = user.Ci,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            }
        };
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<UserInfoDTO> GetUserInfoAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            throw new KeyNotFoundException("Usuario no encontrado");

        var roles = await _userManager.GetRolesAsync(user);

        return new UserInfoDTO
        {
            Id = user!.Id,
            Email = user.Email!,
            Name = user!.Name,
            LastName = user!.LastName,
            Ci = user!.Ci,
            Roles = roles.ToList()
        };
    }
}
