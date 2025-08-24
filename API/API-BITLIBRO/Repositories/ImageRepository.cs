using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.Interfaces.Repositories;
using API_BITLIBRO.Models;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Repositories;

public class ImageRepository : IImageRepository
{
    private readonly AppDbContext _context;

     public ImageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<Image> images)
        {
            await _context.Images.AddRangeAsync(images);
        }

        public async Task<Image?> GetByIdAsync(int id)
        {
            return await _context.Images.FindAsync(id);
        }

        public async Task<List<Image>> GetByBookIdAsync(int bookId)
        {
            return await _context.Images
                .Where(i => i.BookId == bookId)
                .ToListAsync();
        }

        public async Task<Image?> GetByUuidAsync(string imageUuid)
        {
            return await _context.Images
                .FirstOrDefaultAsync(i => i.ImageUuid == imageUuid);
        }

        public void Remove(Image image)
        {
            _context.Images.Remove(image);
        }

        public void RemoveRange(IEnumerable<Image> images)
        {
            _context.Images.RemoveRange(images);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
}
