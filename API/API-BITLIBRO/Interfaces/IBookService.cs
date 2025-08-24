using System;
using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.Book;

namespace API_BITLIBRO.Interfaces;

public interface IBookService
{
    public Task<PagedResponse<ResponseBookDTO>> GetAllBooksAsync(QueryParamsBookDTO queryParams);
    public Task<ResponseBookDTO> GetBookByIdAsync(int id);
    public Task<ResponseBookDTO> CreateBookAsync(CreateBookDTO createDto);
    public Task<ResponseBookDTO> UpdateBookAsync(UpdateBookDTO updateDto);
    public Task<bool> DeleteBookAsync(int id);
    public Task DeleteImageFromBookAsync(int bookId, int imageId);
    public Task DeleteAllImagesAsync(int bookId);
}
