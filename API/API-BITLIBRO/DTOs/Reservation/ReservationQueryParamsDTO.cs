using System;

namespace API_BITLIBRO.DTOs.Reservation;

public class ReservationQueryParamsDTO
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
    public string? ClientName { get; set; }
    public string? EmployeeName { get; set; }
}
