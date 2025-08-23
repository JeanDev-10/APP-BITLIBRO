using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.Interfaces.Repositories;
using API_BITLIBRO.Models;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Repositories;

public class GenreRepository : IGenreRepository
{
    private readonly AppDbContext _context;

    public GenreRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Genre> Items, int TotalRecords)> GetPagedAsync(
        string? nameFilter, int page, int pageSize)
    {
        var query = _context.Genres.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var term = nameFilter.Trim();
            query = query.Where(g => g.Name.Contains(term));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(g => g.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<Genre?> GetByIdAsync(int id) =>
        _context.Genres.FindAsync(id).AsTask(); // tracked (útil para Update/Delete)

    public async Task<bool> ExistsByNameAsync(string name, int? excludingId = null)
    {
        var q = _context.Genres.AsNoTracking().Where(g => g.Name == name);
        if (excludingId.HasValue) q = q.Where(g => g.Id != excludingId.Value);
        return await q.AnyAsync();
    }

    public async Task AddAsync(Genre genre)
    {
        await _context.Genres.AddAsync(genre);
    }

    public Task UpdateAsync(Genre genre)
    {
        _context.Genres.Update(genre);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Genre genre)
    {
        _context.Genres.Remove(genre);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
