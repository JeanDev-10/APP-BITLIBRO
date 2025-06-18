using System;
using API_BITLIBRO.DTOs.Book;
using FluentValidation;

namespace API_BITLIBRO.Validators.Book;

public class ImageValidatorDTO:AbstractValidator<ImageDTO>
{
    public ImageValidatorDTO()
    {
        RuleFor(x => x.File)
                .NotNull().WithMessage("Debe proporcionar un archivo")
                .Must(file => 
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(file!.FileName).ToLower();
                    return allowedExtensions.Contains(extension);
                }).WithMessage("Solo se permiten archivos JPG, JPEG o PNG")
                .Must(file => file!.Length <= 2 * 1024 * 1024)
                .WithMessage("El tamaño máximo permitido es 2MB");
    }
}
