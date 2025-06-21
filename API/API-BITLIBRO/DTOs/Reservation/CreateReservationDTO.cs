using System;

namespace API_BITLIBRO.DTOs.Reservation;

public class CreateReservationDTO
{
    public int BookId { get; set; }
    public string? UserId { get; set; } // Opcional - para cliente existente
    // Opcional - para crear nuevo cliente
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? Ci { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
