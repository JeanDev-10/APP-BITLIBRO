using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs.Book;
using API_BITLIBRO.Interfaces.Repositories;
using API_BITLIBRO.Models;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Repositories;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;
    public BookRepository(AppDbContext context) => _context = context;
    public async Task<(IReadOnlyList<Book> Items, int Total)> GetPagedAsync(QueryParamsBookDTO queryParams)
    {
        var query = _context.Books
            .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
            .Include(b => b.Images)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.Name))
            query = query.Where(b => b.Name.Contains(queryParams.Name));

        if (!string.IsNullOrWhiteSpace(queryParams.Author))
            query = query.Where(b => b.Author.Contains(queryParams.Author));

        if (!string.IsNullOrWhiteSpace(queryParams.ISBN))
            query = query.Where(b => b.ISBN.Contains(queryParams.ISBN));

        if (queryParams.GenreId.HasValue)
            query = query.Where(b => b.BookGenres.Any(bg => bg.GenreId == queryParams.GenreId.Value));

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(b => b.Name)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return (items, total);
    }
    public Task<Book?> GetByIdWithDetailsAsync(int id) =>
    _context.Books
        .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
        .Include(b => b.Images)
        .AsNoTracking()
        .FirstOrDefaultAsync(b => b.Id == id);
    public async Task<bool> GenresExistAsync(IEnumerable<int> genreIds)
    {
        var ids = genreIds.Distinct().ToList();
        var count = await _context.Genres.AsNoTracking()
            .CountAsync(g => ids.Contains(g.Id));
        return count == ids.Count;
    }
     public async Task AddAsync(Book book)
    {
        await _context.Books.AddAsync(book);
    }

    public Task UpdateAsync(Book book)
    {
        _context.Books.Update(book);
        return Task.CompletedTask;
    }

    public async Task<Book?> FindTrackedAsync(int id)
    {
        return await _context.Books
            .Include(b => b.BookGenres)
            .Include(b => b.Images)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public Task DeleteAsync(Book book)
    {
        _context.Books.Remove(book);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
