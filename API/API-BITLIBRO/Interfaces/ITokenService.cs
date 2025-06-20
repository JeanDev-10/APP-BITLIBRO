using System;
using API_BITLIBRO.Models;

namespace API_BITLIBRO.Interfaces;

public interface ITokenService
{
    public Task<string> GenerateToken(User user);
}
