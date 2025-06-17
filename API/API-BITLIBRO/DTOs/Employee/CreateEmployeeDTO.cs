using System;

namespace API_BITLIBRO.DTOs.Employee;

public class CreateEmployeeDTO
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Ci { get; set; } = string.Empty;
}
