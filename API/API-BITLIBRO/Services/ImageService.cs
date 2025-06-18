using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Services;

public class ImageService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ImageService(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task UploadImagesAsync(int bookId, List<IFormFile> images)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var newImages = new List<Image>();
        foreach (var image in images)
        {
            var imageUuid = Guid.NewGuid().ToString();
            var extension = Path.GetExtension(image.FileName).ToLower();
            var fileName = $"{imageUuid}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            newImages.Add(new Image
            {
                ImageUuid = imageUuid,
                Url = $"/uploads/{fileName}",
                BookId = bookId
            });
        }
        _context.Images.AddRange(newImages);
    }

    public async Task DeleteImageAsync(int imageId)
    {
        var image = await _context.Images.FindAsync(imageId);
        if (image == null) throw new KeyNotFoundException($"No se encontró la imagen con ID {imageId}.");;

        var filePath = Path.Combine(_environment.WebRootPath, image.Url.TrimStart('/'));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _context.Images.Remove(image);
        await _context.SaveChangesAsync();

    }

    public async Task DeleteAllImagesAsync(int bookId)
    {
        var images = await _context.Images.Where(i => i.BookId == bookId).ToListAsync();
        if (!images.Any()) throw new KeyNotFoundException($"No se encontró la imagen del bookcon ID {bookId}.");;

        foreach (var image in images)
        {
            var filePath = Path.Combine(_environment.WebRootPath, image.Url.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        _context.Images.RemoveRange(images);
        await _context.SaveChangesAsync();
    }

    public async Task<string> GetImageUrlAsync(string imageUuid)
    {
        var image = await _context.Images.FirstOrDefaultAsync(i => i.ImageUuid == imageUuid);
        return image?.Url;
    }
}
