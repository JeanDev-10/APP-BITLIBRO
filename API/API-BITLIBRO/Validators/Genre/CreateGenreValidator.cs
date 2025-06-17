using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs.Genre;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Validators.Genre;

public class CreateGenreValidator : AbstractValidator<CreateGenreDTO>
{
    public CreateGenreValidator(AppDbContext context)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres")
            .MustAsync(async (name, cancellation) =>
                !await context.Genres.AnyAsync(g => g.Name == name, cancellation))
            .WithMessage("El nombre del género ya existe");
    }
}
