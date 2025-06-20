using System;

namespace API_BITLIBRO.DTOs.Reservation;

public class UpdateReservationDTO
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Pendiente";
}
