using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs.Genre;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Validators.Genre;

public class UpdateGenreValidator : AbstractValidator<UpdateGenreDTO>
{
    private readonly AppDbContext _context;
    public UpdateGenreValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID es requerido");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres")
            .Must((dto,name)=>BeUniqueName(dto.Id,name)).WithMessage("El nombre del género ya existe");
    }
    private bool BeUniqueName(int id, string name)
    {
        return !_context.Genres.AsNoTracking().Any(g => g.Name == name && g.Id != id);
    }
}
