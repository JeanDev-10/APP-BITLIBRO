using System;

namespace API_BITLIBRO.DTOs.Employee;

public class EmployeeResponseDTO
{
    public string Id { get; set; }=string.Empty;
    public string Email { get; set; }=string.Empty;
    public string Name { get; set; }=string.Empty;
    public string LastName { get; set; }=string.Empty;
    public string Ci { get; set; }=string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
