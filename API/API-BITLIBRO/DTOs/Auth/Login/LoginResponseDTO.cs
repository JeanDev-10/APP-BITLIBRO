using System;
using API_BITLIBRO.DTOs.Auth.Me;

namespace API_BITLIBRO.DTOs.Auth.Login;

public class LoginResponseDTO
{
    public string Token { get; set; }=string.Empty;
    public UserInfoDTO? User { get; set; }
}
