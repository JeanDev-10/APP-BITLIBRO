using System;
using System.ComponentModel.DataAnnotations;
using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.Book;
using API_BITLIBRO.DTOs.Genre;
using API_BITLIBRO.Interfaces;
using API_BITLIBRO.Interfaces.Repositories;
using API_BITLIBRO.Interfaces.Transactions;
using API_BITLIBRO.Models;
using API_BITLIBRO.Validators.Book;

namespace API_BITLIBRO.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IImageService _imageService;
    private readonly IUnitOfWork _uow;

    public BookService(IBookRepository bookRepository, IImageService imageService, IUnitOfWork uow)
    {
        _bookRepository = bookRepository;
        _imageService = imageService;
        _uow = uow;
    }

    public async Task<PagedResponse<ResponseBookDTO>> GetAllBooksAsync(QueryParamsBookDTO queryParams)
    {
        var (items, total) = await _bookRepository.GetPagedAsync(queryParams);

        var data = items.Select(b => new ResponseBookDTO
        {
            Id = b.Id,
            Name = b.Name,
            ISBN = b.ISBN,
            Author = b.Author,
            YearPublished = b.YearPublished,
            Editorial = b.Editorial,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt,
            Genres = b.BookGenres!.Select(bg => new GenreResponseDto
            {
                Id = bg.Genre!.Id,
                Name = bg.Genre.Name,
                CreatedAt = bg.Genre.CreatedAt,
                UpdatedAt = bg.Genre.UpdatedAt
            }).ToList(),
            ImageUrls = b.Images!.Select(i => new ResponseImageDTO { Id = i.Id, Url = i.Url }).ToList()
        }).ToList();

        return new PagedResponse<ResponseBookDTO>
        {
            Data = data,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling(total / (double)queryParams.PageSize)
        };
    }
    public async Task<ResponseBookDTO> GetBookByIdAsync(int id)
    {
        var book = await _bookRepository.GetByIdWithDetailsAsync(id);
        if (book is null) return null;

        return new ResponseBookDTO
        {
            Id = book.Id,
            Name = book.Name,
            ISBN = book.ISBN,
            Author = book.Author,
            YearPublished = book.YearPublished,
            Editorial = book.Editorial,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt,
            Genres = book.BookGenres!.Select(bg => new GenreResponseDto
            {
                Id = bg.Genre!.Id,
                Name = bg.Genre.Name,
                CreatedAt = bg.Genre.CreatedAt,
                UpdatedAt = bg.Genre.UpdatedAt
            }).ToList(),
            ImageUrls = book.Images!.Select(i => new ResponseImageDTO { Id = i.Id, Url = i.Url }).ToList()
        };
    }
    public async Task<ResponseBookDTO> CreateBookAsync(CreateBookDTO createDto)
    {
        // Validar las imágenes antes de procesarlas
        var imageValidator = new ImageValidatorDTO();

        foreach (var image in createDto.Images)
        {
            var imageDto = new ImageDTO { File = image };
            var validationResult = await imageValidator.ValidateAsync(imageDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }
        }
        // 2) Validar géneros
        var okGenres = await _bookRepository.GenresExistAsync(createDto.GenreIds);
        if (!okGenres) throw new Exception("Uno o más géneros no existen");

        await _uow.BeginTransactionAsync();
        try
        {
            // 3) Crear libro
            var book = new Book
            {
                Name = createDto.Name!,
                ISBN = createDto.ISBN!,
                Author = createDto.Author,
                YearPublished = createDto.YearPublished,
                Editorial = createDto.Editorial,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                BookGenres = createDto.GenreIds.Select(gid => new BookGenre { GenreId = gid }).ToList()
            };

            await _bookRepository.AddAsync(book);
            await _uow.SaveChangesAsync();

            // 4) Subir imágenes (si hay)
            if (createDto.Images != null && createDto.Images.Count > 0)
                await _imageService.UploadImagesAsync(book.Id, createDto.Images);

            await _uow.SaveChangesAsync();
            await _uow.CommitAsync();

            var result = await _bookRepository.GetByIdWithDetailsAsync(book.Id);
            // no debería ser null
            return await GetBookByIdAsync(book.Id) ?? throw new Exception("Error al consultar el libro recién creado");
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync();
            throw new Exception($"Error al crear el libro: {ex.Message}", ex);
        }

    }
    public async Task<ResponseBookDTO> UpdateBookAsync(UpdateBookDTO updateDto)
    {
        // Validar las imágenes antes de procesarlas
        if (updateDto.Images != null && updateDto.Images.Count > 0)
        {
            var imageValidator = new ImageValidatorDTO();

            foreach (var image in updateDto.Images)
            {
                var imageDto = new ImageDTO { File = image };
                var validationResult = await imageValidator.ValidateAsync(imageDto);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                }
            }
        }
        await _uow.BeginTransactionAsync();
        try
        {
            // 2) Traer book tracked
            var book = await _bookRepository.FindTrackedAsync(updateDto.Id);
            if (book is null) return null;

            // 3) Validar géneros
            var okGenres = await _bookRepository.GenresExistAsync(updateDto.GenreIds);
            if (!okGenres) throw new Exception("Uno o más géneros no existen");

            // 4) Actualizar campos base
            book.Name = updateDto.Name;
            book.ISBN = updateDto.ISBN;
            book.Author = updateDto.Author;
            book.YearPublished = updateDto.YearPublished;
            book.Editorial = updateDto.Editorial;
            book.UpdatedAt = DateTime.UtcNow;

            // 5) Actualizar géneros (reemplazo de la colección)
            book.BookGenres = updateDto.GenreIds
                .Select(gid => new BookGenre { BookId = book.Id, GenreId = gid })
                .ToList();

            await _bookRepository.UpdateAsync(book);
            await _uow.SaveChangesAsync();
            // Procesar imágenes si se proporcionaron
            if (updateDto.Images != null && updateDto.Images.Count > 0)
            {
                await _imageService.UploadImagesAsync(book.Id, updateDto.Images);
                await _uow.SaveChangesAsync();
            }

            await _uow.CommitAsync();

            return await GetBookByIdAsync(book.Id);
        }
        catch (Exception ex)
        {
            await _uow.RollbackAsync();
            throw new Exception($"Error al actualizar el libro: {ex.Message}", ex);
        }
    }
    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await _bookRepository.FindTrackedAsync(id);
        if (book is null) return false;

        // Primero eliminar imágenes (negocio)
        await _imageService.DeleteAllImagesAsync(id);

        await _bookRepository.DeleteAsync(book);
        await _uow.SaveChangesAsync();
        return true;
    }
    public async Task DeleteImageFromBookAsync(int bookId, int imageId)
    {
        var book = await _bookRepository.FindTrackedAsync(bookId);
        if (book == null)
            throw new KeyNotFoundException("Book not found");

        // Aquí delegas en el ImageService
        await _imageService.DeleteImageAsync(imageId);
    }
      public async Task DeleteAllImagesAsync(int bookId)
    {
        var book = await _bookRepository.FindTrackedAsync(bookId);
        if (book == null)
            throw new KeyNotFoundException("Book not found");

        // Delegas en el ImageService
        await _imageService.DeleteAllImagesAsync(bookId);
    }
}
