using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.ApiResponse;
using API_BITLIBRO.DTOs.Book;
using API_BITLIBRO.DTOs.Client;
using API_BITLIBRO.DTOs.Employee;
using API_BITLIBRO.DTOs.Genre;
using API_BITLIBRO.DTOs.Reservation;
using API_BITLIBRO.Interfaces;
using API_BITLIBRO.Interfaces.Repositories;
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;

    public ClientService(IClientRepository repo)
    {
        _clientRepository = repo;
    }

    public async Task<PagedResponse<ClientResponseDTO>> GetUsersAsync(ClientQueryParamsDTO queryParams)
    {
        var (items, total) = await _clientRepository.GetPagedClientsAsync(queryParams);
        var users = items.Select(u => new ClientResponseDTO
        {
            Id = u.Id,
            Ci = u.Ci,
            Name = u.Name,
            LastName = u.LastName,
            CreatedAt = u.CreatedAt
        }).ToList();
        return new PagedResponse<ClientResponseDTO>
        {
            Data = users,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling(total / (double)queryParams.PageSize)
        };
    }

    public async Task<ClientResponseDTO> GetUserAsync(string userId)
    {
        var user = await _clientRepository.GetClientByIdAsync(userId, ensureClientRole: true);
        if (user is null) return null;
        return new ClientResponseDTO
        {
            Id = user!.Id,
            Ci = user!.Ci,
            Name = user!.Name,
            LastName = user!.LastName,
            CreatedAt = user!.CreatedAt
        };
    }
    public async Task<PagedResponse<ReservationResponseDTO>> GetUserWithLoansAsync(string userId, ClientReservationQueryParamsDTO queryParams)
    {
        var (items, totalRecords) = await _clientRepository.GetClientReservationsPagedAsync(userId, queryParams);

        var reservations = items.Select(r => new ReservationResponseDTO
        {
            Id = r.Id,
            Status = r.Status,
            Employee = new EmployeeResponseDTO
            {
                Id = r.Employee!.Id,
                Name = r.Employee.Name,
                LastName = r.Employee.LastName,
                Ci = r.Employee!.Ci,
                Email = r.Employee.Email!,
                CreatedAt = r.Employee.CreatedAt,
                UpdatedAt = r.Employee.UpdatedAt
            },
            Book = new ResponseBookDTO
            {
                Id = r.Book!.Id,
                Name = r.Book.Name,
                ISBN = r.Book.ISBN,
                YearPublished = r.Book.YearPublished,
                Editorial = r.Book.Editorial,
                Author = r.Book.Author,
                CreatedAt = r.Book.CreatedAt,
                UpdatedAt = r.Book.UpdatedAt,
                Genres = r.Book.BookGenres!.Select(bg => new GenreResponseDto
                {
                    Id = bg.Genre!.Id,
                    Name = bg.Genre.Name,
                    CreatedAt = bg.Genre.CreatedAt,
                    UpdatedAt = bg.Genre.UpdatedAt
                }).ToList(),
                ImageUrls = r.Book.Images!.Select(i => new ResponseImageDTO
                {
                    Id = i.Id,
                    Url = i.Url
                }).ToList()
            },
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();
        return new PagedResponse<ReservationResponseDTO>
        {
            Data = reservations,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)queryParams.PageSize)
        };
    }

}
