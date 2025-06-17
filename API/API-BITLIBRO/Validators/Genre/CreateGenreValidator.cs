using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs.Genre;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Validators.Genre;

public class CreateGenreValidator : AbstractValidator<CreateGenreDTO>
{
    private readonly AppDbContext _context;
    public CreateGenreValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres")
            .Must(BeUniqueName).WithMessage("El nombre del género ya existe");
    }

    private bool BeUniqueName(string name)
    {
        return !_context.Genres.Any(g => g.Name == name);
    }
}
