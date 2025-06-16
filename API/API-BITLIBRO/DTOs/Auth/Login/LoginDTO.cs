using System;

namespace API_BITLIBRO.DTOs.Auth.Login;

public class LoginDTO
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
