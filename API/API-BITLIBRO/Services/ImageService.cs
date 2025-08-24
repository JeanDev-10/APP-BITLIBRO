using API_BITLIBRO.Interfaces;
using API_BITLIBRO.Interfaces.Repositories;
using API_BITLIBRO.Models;

namespace API_BITLIBRO.Services;

public class ImageService : IImageService
{
    private readonly IImageRepository _imageRepository;
    private readonly IWebHostEnvironment _environment;

    public ImageService(IImageRepository imageRepository, IWebHostEnvironment environment)
    {
        _imageRepository = imageRepository;
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
        await _imageRepository.AddRangeAsync(newImages);
        await _imageRepository.SaveChangesAsync();
    }

    public async Task DeleteImageAsync(int imageId)
    {
        var image = await _imageRepository.GetByIdAsync(imageId);
        if (image == null) throw new KeyNotFoundException($"No se encontró la imagen con ID {imageId}."); ;

        var filePath = Path.Combine(_environment.WebRootPath, image.Url.TrimStart('/'));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _imageRepository.Remove(image);
        await _imageRepository.SaveChangesAsync();

    }

    public async Task DeleteAllImagesAsync(int bookId)
    {
        var images = await _imageRepository.GetByBookIdAsync(bookId);
        if (!images.Any()) throw new KeyNotFoundException($"No se encontró la imagen del bookcon ID {bookId}."); ;

        foreach (var image in images)
        {
            var filePath = Path.Combine(_environment.WebRootPath, image.Url.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        _imageRepository.RemoveRange(images);
        await _imageRepository.SaveChangesAsync();
    }

    public async Task<string> GetImageUrlAsync(string imageUuid)
    {
        var image = await _imageRepository.GetByUuidAsync(imageUuid);
        return image?.Url;
    }
}
