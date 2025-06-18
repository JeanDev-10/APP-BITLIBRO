using System;

namespace API_BITLIBRO.DTOs.Book;

public class UpdateBookDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string YearPublished { get; set; } = string.Empty;
    public string Editorial { get; set; } = string.Empty;
    public List<int> GenreIds { get; set; } = new List<int>();
    public List<IFormFile> Images { get; set; } = new List<IFormFile>();    
}
