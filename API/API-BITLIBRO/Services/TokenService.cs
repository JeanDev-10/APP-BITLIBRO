using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API_BITLIBRO.Services;

public class TokenService

{
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager; //gestionar todo relacionado al usuario
    public TokenService(IConfiguration config, UserManager<User> userManager)
    {
        _config = config;
        _userManager = userManager;
    }
    public async Task<string> GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["JWTSettings:SecretKey"]!);
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Email, user.Email!),
            new (JwtRegisteredClaimNames.Name, user.Name!),
            new (JwtRegisteredClaimNames.NameId, user.Id!),
            new Claim("name", user.Name),
            new Claim("lastname", user.LastName),
            new Claim("ci", user.Ci),
            new (JwtRegisteredClaimNames.Aud, _config["JWTSettings:Audience"]!),
            new (JwtRegisteredClaimNames.Iss, _config["JWTSettings:Issuer"]!)
        };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JWTSettings:TokenExpirationInMinutes"])),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
