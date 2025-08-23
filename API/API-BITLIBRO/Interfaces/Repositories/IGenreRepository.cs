using System;
using API_BITLIBRO.Models;

namespace API_BITLIBRO.Interfaces.Repositories;

public interface IGenreRepository
{
    Task<(IReadOnlyList<Genre> Items, int TotalRecords)> GetPagedAsync(
            string? nameFilter, int page, int pageSize);

    Task<Genre?> GetByIdAsync(int id);

    Task<bool> ExistsByNameAsync(string name, int? excludingId = null);

    Task AddAsync(Genre genre);
    Task UpdateAsync(Genre genre);
    Task DeleteAsync(Genre genre);

    Task<int> SaveChangesAsync();
}
