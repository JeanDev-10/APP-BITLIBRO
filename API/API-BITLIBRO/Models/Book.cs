using System;
using System.ComponentModel.DataAnnotations;

namespace API_BITLIBRO.Models;

public class Book
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }=string.Empty;

    [Required]
    [MaxLength(20)]
    public string ISBN { get; set; }=string.Empty;

    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(4)]
    public string YearPublished { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Editorial { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    //relacion mucho a mucho
    public ICollection<BookGenre>? BookGenres { get; set; } 
    //relacion uno a muchos con imagenes
    public ICollection<Image>? Images { get; set; }
}
