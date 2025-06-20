using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API_BITLIBRO.DTOs.Client;
using API_BITLIBRO.DTOs.Employee;

namespace API_BITLIBRO.Models;

public class Reservation
{
    public int Id { get; set; }
    public string Status { get; set; } = "Pendiente"; // Pendiente, Finalizado
    public int BookId { get; set; }
    public Book? Book { get; set; }

    public string? EmployeeId { get; set; }

    [ForeignKey("EmployeeId")]
    public User? Employee { get; set; }

    public string? ClientId { get; set; }

    [ForeignKey("ClientId")]
    public User? Client { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
