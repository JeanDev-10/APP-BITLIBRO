using System;

namespace API_BITLIBRO.DTOs.Genre;

public class GenreQueryParams
{
    public string? Name { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
