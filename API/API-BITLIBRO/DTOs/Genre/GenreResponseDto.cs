using System;

namespace API_BITLIBRO.DTOs.Genre;

public class GenreResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }=string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
