using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.Genre;
using API_BITLIBRO.Models;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Services;

public class GenreService
{
    private readonly AppDbContext _context;

    public GenreService(AppDbContext context)
    {
        _context = context;
    }


    public async Task<PagedResponse<GenreResponseDto>> GetAllGenresAsync(GenreQueryParams queryParams)
    {
        var query = _context.Genres.AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Name))
        {
            query = query.Where(g => g.Name.Contains(queryParams.Name));
        }

        var totalRecords = await query.CountAsync();

        var genres = await query
            .OrderBy(g => g.Name)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(g => new GenreResponseDto
            {
                Id = g.Id,
                Name = g.Name,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt
            })
            .ToListAsync();

        return new PagedResponse<GenreResponseDto>
        {
            Data = genres,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)queryParams.PageSize)
        };
    }

    public async Task<GenreResponseDto> GetGenreByIdAsync(int id)
    {
        var genre = await _context.Genres.FindAsync(id);

        if (genre == null) return null;

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
        var genre = new Genre
        {
            Name = createDto.Name
        };

        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();

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
        var genre = await _context.Genres.FindAsync(updateDto.Id);
        if (genre == null) return null;

        genre.Name = updateDto.Name;
        genre.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

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
        var genre = await _context.Genres.FindAsync(id);
        if (genre == null) return false;

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();

        return true;
    }
}
