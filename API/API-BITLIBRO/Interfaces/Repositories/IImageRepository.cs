using System;
using API_BITLIBRO.Models;

namespace API_BITLIBRO.Interfaces.Repositories;

public interface IImageRepository
{
    Task AddRangeAsync(IEnumerable<Image> images);
    Task<Image?> GetByIdAsync(int id);
    Task<List<Image>> GetByBookIdAsync(int bookId);
    Task<Image?> GetByUuidAsync(string imageUuid);
    void Remove(Image image);
    void RemoveRange(IEnumerable<Image> images);
    Task SaveChangesAsync();
}
