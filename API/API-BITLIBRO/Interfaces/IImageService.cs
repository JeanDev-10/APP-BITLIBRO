using System;

namespace API_BITLIBRO.Interfaces;

public interface IImageService
{
    public Task UploadImagesAsync(int bookId, List<IFormFile> images);
    public Task DeleteImageAsync(int imageId);
    public Task DeleteAllImagesAsync(int bookId);
    public Task<string> GetImageUrlAsync(string imageUuid);

}