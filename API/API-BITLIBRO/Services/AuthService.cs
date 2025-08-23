using System;
using API_BITLIBRO.DTOs.Auth.Login;
using API_BITLIBRO.DTOs.Auth.Me;
using API_BITLIBRO.Interfaces;
using API_BITLIBRO.Interfaces.Repositories;

namespace API_BITLIBRO.Services;

public class AuthService:IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IAuthRepository _authRepository;
    public AuthService(IAuthRepository authRepository, ITokenService tokenService)
    {
        _tokenService = tokenService;
        _authRepository = authRepository;
    }
    public async Task<LoginResponseDTO> LoginAsync(LoginDTO loginDTO)
    {
        var user = await _authRepository.FindByEmailAsync(loginDTO.Email);

        if (user == null || _authRepository.CheckPasswordAsync(user, loginDTO.Password).Result == false)
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
                Roles = (await _authRepository.GetRolesAsync(user)).ToList()
            }
        };
    }

    public async Task LogoutAsync()
    {
        await _authRepository.SignOutAsync();
    }

    public async Task<UserInfoDTO> GetUserInfoAsync(string userId)
    {
        var user = await _authRepository.FindByIdAsync(userId);

        if (user == null)
            throw new KeyNotFoundException("Usuario no encontrado");

        var roles = await _authRepository.GetRolesAsync(user);

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
