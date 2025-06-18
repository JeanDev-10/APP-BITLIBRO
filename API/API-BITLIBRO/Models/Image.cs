using System;

namespace API_BITLIBRO.Models;

public class Image
{
    public int Id { get; set; }

    public string ImageUuid { get; set; }= string.Empty;
    public string Url { get; set; }= string.Empty;

    public int BookId { get; set; }
    public Book? Book { get; set; }
}
