using System;

namespace API_BITLIBRO.DTOs.Client;

public class ClientResponseDTO
{
    public string Id { get; set; }=string.Empty;
    public string Ci { get; set; }=string.Empty;
    public string Name { get; set; }=string.Empty;
    public string LastName { get; set; }=string.Empty;
    public DateTime CreatedAt { get; set; }
}
