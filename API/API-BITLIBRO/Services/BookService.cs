using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.Book;
using API_BITLIBRO.DTOs.Genre;
using API_BITLIBRO.Models;
using API_BITLIBRO.Validators.Book;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Services;

public class BookService
{
    private readonly AppDbContext _context;
    private readonly ImageService _imageService;

    public BookService(AppDbContext context, ImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<PagedResponse<ResponseBookDTO>> GetAllBooksAsync(QueryParamsBookDTO queryParams)
    {
        var query = _context.Books
            .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
            .Include(b => b.Images)
            .AsQueryable();
        // Aplicar filtros
        if (!string.IsNullOrEmpty(queryParams.Name))
            query = query.Where(b => b.Name.Contains(queryParams.Name));

        if (!string.IsNullOrEmpty(queryParams.Author))
            query = query.Where(b => b.Author.Contains(queryParams.Author));

        if (!string.IsNullOrEmpty(queryParams.ISBN))
            query = query.Where(b => b.ISBN.Contains(queryParams.ISBN));

        if (queryParams.GenreId.HasValue)
            query = query.Where(b => b.BookGenres.Any(bg => bg.GenreId == queryParams.GenreId.Value));

        // Contar total antes de paginar
        var totalRecords = await query.CountAsync();

        // Aplicar paginación
        var books = await query
            .OrderBy(b => b.Name)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(b => new ResponseBookDTO
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
                ImageUrls = b.Images!.Select(i => new ResponseImageDTO
                {
                    Id = i.Id,
                    Url = i.Url
                }).ToList()
            })
            .ToListAsync();

        return new PagedResponse<ResponseBookDTO>
        {
            Data = books,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)queryParams.PageSize)
        };
    }
    public async Task<ResponseBookDTO> GetBookByIdAsync(int id)
    {
        var book = await _context.Books
            .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
            .Include(b => b.Images)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null) return null;

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
            ImageUrls = book.Images!.Select(i => new ResponseImageDTO
            {
                Id = i.Id,
                Url = i.Url
            }).ToList()
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
                throw new Exception(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }
        }
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {

            // Validar que los géneros existen
            var existingGenres = await _context.Genres
                .Where(g => createDto.GenreIds.Contains(g.Id))
                .Select(g => g.Id)
                .ToListAsync();

            if (existingGenres.Count != createDto.GenreIds.Count)
                throw new Exception("Uno o más géneros no existen");

            // Crear el libro
            var book = new Book
            {
                Name = createDto.Name!,
                ISBN = createDto.ISBN!,
                Author = createDto.Author,
                YearPublished = createDto.YearPublished,
                Editorial = createDto.Editorial,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                BookGenres = createDto.GenreIds.Select(genreId => new BookGenre { GenreId = genreId }).ToList()
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            // Procesar imágenes
            if (createDto.Images != null && createDto.Images.Count > 0)
            {
                await _imageService.UploadImagesAsync(book.Id, createDto.Images);
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return await GetBookByIdAsync(book.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Error al crear el libro: {ex.Message}", ex); // Pasa el inner exception
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
                    throw new Exception(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                }
            }
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var book = await _context.Books
                .Include(b => b.BookGenres)
                .Include(b => b.Images)
                .FirstOrDefaultAsync(b => b.Id == updateDto.Id);

            if (book == null) return null;

            // Validar que los géneros existen
            var existingGenres = await _context.Genres
                .Where(g => updateDto.GenreIds.Contains(g.Id))
                .Select(g => g.Id)
                .ToListAsync();

            if (existingGenres.Count != updateDto.GenreIds.Count)
                throw new Exception("Uno o más géneros no existen");

            // Actualizar propiedades
            book.Name = updateDto.Name;
            book.ISBN = updateDto.ISBN;
            book.Author = updateDto.Author;
            book.YearPublished = updateDto.YearPublished;
            book.Editorial = updateDto.Editorial;
            book.UpdatedAt = DateTime.UtcNow;

            // Actualizar géneros
            book.BookGenres = updateDto.GenreIds
                .Select(genreId => new BookGenre { BookId = book.Id, GenreId = genreId })
                .ToList();
            // Procesar imágenes si se proporcionaron
            if (updateDto.Images != null && updateDto.Images.Count > 0)
            {
                await _imageService.DeleteAllImagesAsync(book.Id);
                await _imageService.UploadImagesAsync(book.Id, updateDto.Images);
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return await GetBookByIdAsync(book.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Error al crear el libro: {ex.Message}", ex); // Pasa el inner exception
        }
    }
    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return false;

        // Eliminar imágenes primero
        await _imageService.DeleteAllImagesAsync(id);

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return true;
    }
}
