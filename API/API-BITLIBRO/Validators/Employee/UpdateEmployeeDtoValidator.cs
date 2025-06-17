using System;
using API_BITLIBRO.DTOs.Employee;
using API_BITLIBRO.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Validators.Employee;

public class UpdateEmployeeDtoValidator:AbstractValidator<UpdateEmployeeDTO>
{
    public UpdateEmployeeDtoValidator(UserManager<User> userManager)
    {
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
                .MustAsync(async (dto, ci, cancellation) => 
                {
                    var user = await userManager.Users.FirstOrDefaultAsync(u => u.Ci == ci);
                    return user == null || user.Id == dto.Id;
                }).WithMessage("El CI ya está registrado");
    }
}
