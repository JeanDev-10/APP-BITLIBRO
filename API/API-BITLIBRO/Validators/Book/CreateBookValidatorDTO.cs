using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs.Book;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Validators.Book;

public class CreateBookValidatorDTO : AbstractValidator<CreateBookDTO>
{
    public CreateBookValidatorDTO(AppDbContext _context)
    {
        RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres")
                   .MustAsync(async (name, _) =>
            {
                return !await _context.Books.AnyAsync(b => b.Name == name.Trim());
            }).WithMessage("Ya existe un libro con ese nombre");

        RuleFor(x => x.ISBN)
               .NotEmpty().WithMessage("El ISBN es requerido")
               .MaximumLength(20).WithMessage("El ISBN no puede exceder los 20 caracteres")
               .MustAsync(async (isbn, _) =>
            {
                return !await _context.Books.AnyAsync(b => b.ISBN == isbn.Trim());
            }).WithMessage("Ya existe un libro con ese ISBN");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("El autor es requerido")
            .MaximumLength(100).WithMessage("El autor no puede exceder los 100 caracteres");

        RuleFor(x => x.YearPublished)
            .NotEmpty().WithMessage("El año es obligatorio")
            .MaximumLength(4).WithMessage("El año debe tener máximo 4 caracteres")
            .Matches(@"^\d{4}$").When(x => !string.IsNullOrEmpty(x.YearPublished))
            .WithMessage("El año debe ser un número de 4 dígitos");

        RuleFor(x => x.Editorial)
        .NotEmpty().WithMessage("La editorial es obligatoria")
         .MaximumLength(100).WithMessage("La editorial no puede exceder los 100 caracteres");

        RuleFor(x => x.GenreIds)
            .NotEmpty().WithMessage("Debe especificar al menos un género");

        RuleFor(x => x.Images)
            .NotEmpty().WithMessage("Debe subir al menos una imagen")
            .Must(images => images.Count <= 3).WithMessage("No puede subir más de 3 imágenes");
    }
}
