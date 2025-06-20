using System;
using API_BITLIBRO.DTOs.Reservation;

namespace API_BITLIBRO.DTOs.Client;

public class ClientLoansResponseDTO:ClientResponseDTO
{
    public List<ReservationResponseDTO>? Reservations { get; set; }
}
