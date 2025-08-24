using System;
using API_BITLIBRO.DTOs.Book;
using API_BITLIBRO.Models;

namespace API_BITLIBRO.Interfaces.Repositories;

public interface IBookRepository
{
    Task<(IReadOnlyList<Book> Items, int Total)> GetPagedAsync(QueryParamsBookDTO query);
    Task<Book?> GetByIdWithDetailsAsync(int id); // BookGenres.Genre + Images
    Task<bool> GenresExistAsync(IEnumerable<int> genreIds);

    Task AddAsync(Book book);
    Task UpdateAsync(Book book);
    Task DeleteAsync(Book book);
    Task<Book?> FindTrackedAsync(int id); // útil para Update/Delete

    Task<int> SaveChangesAsync();
}
