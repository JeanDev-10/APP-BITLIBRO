using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.Genre;
using API_BITLIBRO.Interfaces;
using API_BITLIBRO.Interfaces.Repositories;
using API_BITLIBRO.Models;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Services;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _repo;

    public GenreService(IGenreRepository repo)
    {
        _repo = repo;
    }


    public async Task<PagedResponse<GenreResponseDto>> GetAllGenresAsync(GenreQueryParams queryParams)
    {
        var (items, total) = await _repo.GetPagedAsync(queryParams.Name, queryParams.Page, queryParams.PageSize);

        var dtos = items.Select(g => new GenreResponseDto
        {
            Id = g.Id,
            Name = g.Name,
            CreatedAt = g.CreatedAt,
            UpdatedAt = g.UpdatedAt
        }).ToList();
        return new PagedResponse<GenreResponseDto>
        {
            Data = dtos,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling(total / (double)queryParams.PageSize)
        };
    }

    public async Task<GenreResponseDto> GetGenreByIdAsync(int id)
    {
        var genre = await _repo.GetByIdAsync(id);

        if (genre is null) return null;

        return new GenreResponseDto
        {
            Id = genre.Id,
            Name = genre.Name,
            CreatedAt = genre.CreatedAt,
            UpdatedAt = genre.UpdatedAt
        };
    }

    public async Task<GenreResponseDto> CreateGenreAsync(CreateGenreDTO createDto)
    {
        // Regla de unicidad en Service
        if (await _repo.ExistsByNameAsync(createDto.Name))
            throw new InvalidOperationException("El nombre del género ya existe");

        var genre = new Genre
        {
            Name = createDto.Name
            // CreatedAt/UpdatedAt pueden tener default en el modelo/DB
        };

        await _repo.AddAsync(genre);
        await _repo.SaveChangesAsync();

        return new GenreResponseDto
        {
            Id = genre.Id,
            Name = genre.Name,
            CreatedAt = genre.CreatedAt,
            UpdatedAt = genre.UpdatedAt
        };
    }
    public async Task<GenreResponseDto> UpdateGenreAsync(UpdateGenreDTO updateDto)
    {
        var genre = await _repo.GetByIdAsync(updateDto.Id);
        if (genre is null) return null;

        if (await _repo.ExistsByNameAsync(updateDto.Name, excludingId: updateDto.Id))
            throw new InvalidOperationException("El nombre del género ya existe");

        genre.Name = updateDto.Name;
        genre.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(genre);
        await _repo.SaveChangesAsync();

        return new GenreResponseDto
        {
            Id = genre.Id,
            Name = genre.Name,
            CreatedAt = genre.CreatedAt,
            UpdatedAt = genre.UpdatedAt
        };
    }
    public async Task<bool> DeleteGenreAsync(int id)
    {
        var genre = await _repo.GetByIdAsync(id);
        if (genre is null) return false;

        await _repo.DeleteAsync(genre);
        await _repo.SaveChangesAsync();
        return true;
    }
}
