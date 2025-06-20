using System;
using Microsoft.AspNetCore.Identity;

namespace API_BITLIBRO.Models;

public class User : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Ci { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Relación con Reservation como Cliente (quien recibe el préstamo)
    public virtual ICollection<Reservation> ReservationsAsClient { get; set; } = new List<Reservation>();
    // Relación con Reservation como Empleado (quien gestiona el préstamo)
    public virtual ICollection<Reservation> ReservationsAsEmployee { get; set; } = new List<Reservation>();

}
