using System;
using API_BITLIBRO.DTOs.Auth.Me;
using API_BITLIBRO.DTOs.Book;
using API_BITLIBRO.DTOs.Client;
using API_BITLIBRO.DTOs.Employee;
using API_BITLIBRO.Models;

namespace API_BITLIBRO.DTOs.Reservation;

public class ReservationResponseDTO
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public EmployeeResponseDTO? Employee { get; set; }
    public ClientResponseDTO? Client { get; set; }
    public ResponseBookDTO? Book { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
