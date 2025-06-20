using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs.Reservation;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Validators.Reservation;

public class UpdateReservationValidatorDTO : AbstractValidator<UpdateReservationDTO>
{
    public UpdateReservationValidatorDTO(AppDbContext _context)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID de la reserva es requerido")
            .GreaterThan(0).WithMessage("El ID de la reserva debe ser válido");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Debe proporcionar al menos un estado para actualizar")
            .Matches("^(Pendiente|Finalizado|Cancelado)$").When(x => !string.IsNullOrEmpty(x.Status))
            .WithMessage("El estado debe ser Pendiente, Finalizado o Cancelado");


        RuleFor(x => x.EndDate)
    .MustAsync(async (dto, endDate, context, cancellation) =>
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == dto.Id, cancellation);
        if (reservation == null)
            return false; // o true si quieres ignorar validación si no existe
        return endDate > reservation.StartDate;
    })
    .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio ")
    .MustAsync(async (dto, endDate, context, cancellation) =>
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == dto.Id, cancellation);
        if (reservation == null)
            return false; // o true si quieres ignorar validación si no existe
        return endDate <= reservation.StartDate.AddDays(30);
    })
            .WithMessage("El préstamo no puede exceder los 30 días");
    }
}
