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
        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es requerido").MaximumLength(50).WithMessage("El nombre no puede exceder los 50 caracteres"); ;
        RuleFor(x => x.LastName)
           .NotEmpty().WithMessage("El apellido es requerido")
           .MaximumLength(50).WithMessage("El apellido no puede exceder los 50 caracteres");
        RuleFor(x => x.Ci).NotEmpty();
        RuleFor(x => x.StartDate).LessThanOrEqualTo(x => x.EndDate);
        RuleFor(x => x.Ci)
             .NotEmpty().WithMessage("El CI es requerido")
             .MinimumLength(10).WithMessage("El CI debe tener 10 caracteres")
             .MaximumLength(10).WithMessage("El CI debe tener 10 caracteres")
             .MustAsync(async (ci, cancellation) => (await userManager.Users.FirstOrDefaultAsync(u => u.Ci == ci)) == null) .WithMessage("El CI ya está registrado");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("La fecha de inicio es requerida")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("La fecha de inicio no puede ser en el pasado");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("La fecha de fin es requerida")
            .GreaterThan(x => x.StartDate).WithMessage("La fecha de fin debe ser posterior a la fecha de inicio")
            .LessThanOrEqualTo(x => x.StartDate.AddDays(30)).WithMessage("El préstamo no puede exceder los 30 días");
    }
}
