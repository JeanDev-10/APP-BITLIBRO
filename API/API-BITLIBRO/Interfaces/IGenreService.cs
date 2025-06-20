using System;
using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.Genre;

namespace API_BITLIBRO.Interfaces;

public interface IGenreService
{
    public Task<PagedResponse<GenreResponseDto>> GetAllGenresAsync(GenreQueryParams queryParams);
    public Task<GenreResponseDto> GetGenreByIdAsync(int id);
    public Task<GenreResponseDto> CreateGenreAsync(CreateGenreDTO createDto);
    public Task<GenreResponseDto> UpdateGenreAsync(UpdateGenreDTO updateDto);
    public Task<bool> DeleteGenreAsync(int id);
}
