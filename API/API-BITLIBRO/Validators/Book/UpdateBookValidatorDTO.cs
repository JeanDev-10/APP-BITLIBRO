using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs.Book;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Validators.Book;

public class UpdateBookValidatorDTO : AbstractValidator<UpdateBookDTO>
{
    public UpdateBookValidatorDTO(AppDbContext _context)
    {
        RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres")
                .MustAsync(async (dto, name, _) =>
            {
                return !await _context.Books.AnyAsync(b => b.Name == name && b.Id != dto.Id);
            }).WithMessage("Ya existe un libro con ese nombre");

        RuleFor(x => x.ISBN)
            .NotEmpty().WithMessage("El ISBN es requerido")
            .MaximumLength(20).WithMessage("El ISBN no puede exceder los 20 caracteres")
            .MustAsync(async (dto, isbn, _) =>
            {
                return !await _context.Books.AnyAsync(b => b.ISBN == isbn && b.Id != dto.Id);
            }).WithMessage("Ya existe un libro con ese ISBN");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("El autor es requerido")
            .MaximumLength(100).WithMessage("El autor no puede exceder los 100 caracteres");

        RuleFor(x => x.YearPublished)
            .MaximumLength(4).WithMessage("El año debe tener máximo 4 caracteres")
            .Matches(@"^\d{4}$").When(x => !string.IsNullOrEmpty(x.YearPublished))
            .WithMessage("El año debe ser un número de 4 dígitos");

        RuleFor(x => x.Editorial)
            .MaximumLength(100).WithMessage("La editorial no puede exceder los 100 caracteres");

        RuleFor(x => x.GenreIds)
            .NotEmpty().WithMessage("Debe especificar al menos un género");

        RuleFor(x => x.Images)
            .Must(images => images == null || images.Count <= 3)
            .WithMessage("No puede subir más de 3 imágenes");
    }
}
