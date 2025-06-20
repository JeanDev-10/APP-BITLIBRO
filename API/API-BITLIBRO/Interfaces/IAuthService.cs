using System;
using API_BITLIBRO.DTOs.Auth.Login;
using API_BITLIBRO.DTOs.Auth.Me;

namespace API_BITLIBRO.Interfaces;

public interface IAuthService
{
    public Task<LoginResponseDTO> LoginAsync(LoginDTO loginDTO);
    public Task LogoutAsync();
    public Task<UserInfoDTO> GetUserInfoAsync(string userId);
}
