using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs.Genre;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Validators.Genre;

public class UpdateGenreValidator : AbstractValidator<UpdateGenreDTO>
{
    public UpdateGenreValidator(AppDbContext context)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID es requerido");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres")
            .MustAsync(async (dto, name, cancellation) =>
            {
                return !await context.Genres
                    .AsNoTracking()
                    .AnyAsync(g => g.Name == name && g.Id != dto.Id, cancellation);
            })
            .WithMessage("El nombre del género ya existe");
    }
}
