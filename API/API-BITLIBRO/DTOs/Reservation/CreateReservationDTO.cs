using System;

namespace API_BITLIBRO.DTOs.Reservation;

public class CreateReservationDTO
{
    public int BookId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Ci { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
