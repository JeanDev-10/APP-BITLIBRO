using System;
using API_BITLIBRO.DTOs.Genre;
using API_BITLIBRO.Models;

namespace API_BITLIBRO.DTOs.Book;

public class ResponseBookDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string YearPublished { get; set; } = string.Empty;
    public string Editorial { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<GenreResponseDto> Genres { get; set; } = new List<GenreResponseDto>();
    public List<ResponseImageDTO> ImageUrls { get; set; } = new List<ResponseImageDTO>();

}
