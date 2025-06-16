using System;

namespace API_BITLIBRO.DTOs.Auth.Me;

public class UserInfoDTO
{
    public string Id { get; set; }=string.Empty;
    public string Email { get; set; }=string.Empty;
    public string Name { get; set; }=string.Empty;
    public string LastName { get; set; }=string.Empty;
    public string Ci { get; set; }=string.Empty;
    public IList<string>? Roles { get; set; }
}
