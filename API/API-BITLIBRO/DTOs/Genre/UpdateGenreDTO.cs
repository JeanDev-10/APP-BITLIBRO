using System;

namespace API_BITLIBRO.DTOs.Genre;

public class UpdateGenreDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
