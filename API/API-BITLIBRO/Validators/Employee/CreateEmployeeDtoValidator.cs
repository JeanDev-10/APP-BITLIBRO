using System;
using API_BITLIBRO.DTOs.Employee;
using API_BITLIBRO.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Validators.Employee;

public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDTO>
{
    public CreateEmployeeDtoValidator(UserManager<User> userManager)
    {
        RuleFor(x => x.Email)
        .NotEmpty().WithMessage("El email es requerido")
        .EmailAddress().WithMessage("El email debe ser una dirección de correo electrónico válida")
        .MustAsync(async (email, cancellation) => (await userManager.FindByEmailAsync(email)) == null)
            .WithMessage("El email ya está en uso");
        RuleFor(x => x.Password)
        .NotEmpty().WithMessage("La contraseña es requerida")
        .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres");
        RuleFor(x => x.Name)
               .NotEmpty().WithMessage("El nombre es requerido")
               .MaximumLength(50).WithMessage("El nombre no puede exceder los 50 caracteres");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido")
            .MaximumLength(50).WithMessage("El apellido no puede exceder los 50 caracteres");

        RuleFor(x => x.Ci)
            .NotEmpty().WithMessage("El CI es requerido")
            .MinimumLength(10).WithMessage("El CI debe tener 10 caracteres")
            .MaximumLength(10).WithMessage("El CI debe tener 10 caracteres")
            .MustAsync(async (ci, cancellation) =>
                (await userManager.Users.FirstOrDefaultAsync(u => u.Ci == ci)) == null)
            .WithMessage("El CI ya está registrado");
    }
}
