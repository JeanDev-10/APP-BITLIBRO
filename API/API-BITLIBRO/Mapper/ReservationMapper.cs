using System;
using API_BITLIBRO.DTOs.Book;
using API_BITLIBRO.DTOs.Client;
using API_BITLIBRO.DTOs.Employee;
using API_BITLIBRO.DTOs.Genre;
using API_BITLIBRO.DTOs.Reservation;
using API_BITLIBRO.Models;

namespace API_BITLIBRO.Mapper;

public static class ReservationMapper
{
    public static ReservationResponseDTO ToDto(this Reservation r)
    {
        return new ReservationResponseDTO
        {
            Id = r.Id,
            Status = r.Status,
            Employee = r.Employee == null ? null : new EmployeeResponseDTO
            {
                Id = r.Employee.Id,
                Name = r.Employee.Name,
                LastName = r.Employee.LastName,
                Ci = r.Employee.Ci,
                Email = r.Employee.Email!,
                CreatedAt = r.Employee.CreatedAt,
                UpdatedAt = r.Employee.UpdatedAt
            },
            Book = r.Book == null ? null : new ResponseBookDTO
            {
                Id = r.Book.Id,
                Name = r.Book.Name,
                ISBN = r.Book.ISBN,
                YearPublished = r.Book.YearPublished,
                Editorial = r.Book.Editorial,
                Author = r.Book.Author,
                CreatedAt = r.Book.CreatedAt,
                UpdatedAt = r.Book.UpdatedAt,
                Genres = r.Book.BookGenres?.Select(bg => new GenreResponseDto
                {
                    Id = bg.Genre!.Id,
                    Name = bg.Genre.Name,
                    CreatedAt = bg.Genre.CreatedAt,
                    UpdatedAt = bg.Genre.UpdatedAt
                }).ToList() ?? new List<GenreResponseDto>(),
                ImageUrls = r.Book.Images?.Select(i => new ResponseImageDTO { Id = i.Id, Url = i.Url }).ToList() ?? new List<ResponseImageDTO>()
            },
            Client = r.Client == null ? null : new ClientResponseDTO
            {
                Id = r.Client.Id,
                Name = r.Client.Name,
                LastName = r.Client.LastName,
                Ci = r.Client.Ci,
                CreatedAt = r.Client.CreatedAt
            },
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }
}
