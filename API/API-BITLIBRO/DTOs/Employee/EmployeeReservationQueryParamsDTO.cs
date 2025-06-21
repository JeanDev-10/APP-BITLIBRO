using System;

namespace API_BITLIBRO.DTOs.Employee;

public class EmployeeReservationQueryParamsDTO
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; } // Pendiente, Finalizado, Cancelado
    public string? ClientName { get; set; } // Filtro por nombre o apellido del cliente
    public DateTime? StartDate { get; set; } // Filtro fecha inicial
    public DateTime? EndDate { get; set; } // Filtro fecha final
}
