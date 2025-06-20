using System;

namespace API_BITLIBRO.DTOs.Client;

public class ClientQueryParamsDTO
{
    public string Ci { get; set; }=string.Empty;
    public string Name { get; set; }=string.Empty;
    public string LastName { get; set; }=string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
