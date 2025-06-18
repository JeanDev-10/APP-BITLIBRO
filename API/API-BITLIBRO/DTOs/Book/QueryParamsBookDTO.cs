using System;

namespace API_BITLIBRO.DTOs.Book;

public class QueryParamsBookDTO
{
    public string Name { get; set; }=string.Empty;
    public string Author { get; set; }=string.Empty;
    public string ISBN { get; set; }=string.Empty;
    public int? GenreId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
