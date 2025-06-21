using System;
using API_BITLIBRO.DTOs.Reservation;
using API_BITLIBRO.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Validators.Reservation;

public class CreateReservationValidatorDTO : AbstractValidator<CreateReservationDTO>
{
    public CreateReservationValidatorDTO(UserManager<User> userManager)
    {
        RuleFor(x => x.BookId).NotEmpty().WithMessage("Debe proporcionar el ID del libro");
        RuleFor(x => x.StartDate)
           .NotEmpty().WithMessage("La fecha de inicio es requerida")
           .GreaterThanOrEqualTo(DateTime.Today).WithMessage("La fecha de inicio no puede ser en el pasado");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("La fecha de fin es requerida")
            .GreaterThan(x => x.StartDate).WithMessage("La fecha de fin debe ser posterior a la fecha de inicio")
            .LessThanOrEqualTo(x => x.StartDate.AddDays(30)).WithMessage("El préstamo no puede exceder los 30 días");

        // Regla para validar que se proporcione UserId O datos de usuario, pero no ambos
        RuleFor(x => x)
            .Must(x => (x.UserId != null) ^ (x.Ci != null && x.Name != null && x.LastName != null))
            .WithMessage("Debe proporcionar UserId O datos de usuario (Name, LastName, Ci), pero no ambos");

        // Validaciones cuando se proporcionan datos de usuario
        When(x => x.Ci != null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(50).WithMessage("El nombre no puede exceder los 50 caracteres");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es requerido")
                .MaximumLength(50).WithMessage("El apellido no puede exceder los 50 caracteres");

            RuleFor(x => x.Ci)
                .NotEmpty().WithMessage("El CI es requerido")
                .Length(10).WithMessage("El CI debe tener 10 caracteres")
                .MustAsync(async (ci, cancellation) =>
                    !await userManager.Users.AnyAsync(u => u.Ci == ci))
                .WithMessage("El CI ya está registrado");
        });

        // Validación cuando se proporciona UserId
        When(x => x.UserId != null, () =>
        {
            RuleFor(x => x.UserId)
                .MustAsync(async (userId, cancellation) =>
                    await userManager.Users.AnyAsync(u => u.Id == userId))
                .WithMessage("El usuario no existe");
        });


    }
}
