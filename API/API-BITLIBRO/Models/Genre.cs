using System;
using System.ComponentModel.DataAnnotations;

namespace API_BITLIBRO.Models;

public class Genre
{
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<BookGenre>? BookGenres { get; set; }
}
