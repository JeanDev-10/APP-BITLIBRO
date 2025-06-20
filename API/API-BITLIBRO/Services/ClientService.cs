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
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Services;

public class ClientService : IClientService
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public ClientService(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public async Task<PagedResponse<ClientResponseDTO>> GetUsersAsync(ClientQueryParamsDTO queryParams)
    {
        var query = _userManager.Users.AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrEmpty(queryParams.Ci))
            query = query.Where(u => u.Ci.Contains(queryParams.Ci));

        if (!string.IsNullOrEmpty(queryParams.Name))
            query = query.Where(u => u.Name.Contains(queryParams.Name));

        if (!string.IsNullOrEmpty(queryParams.LastName))
            query = query.Where(u => u.LastName.Contains(queryParams.LastName));

        // Contar total antes de paginar
        var totalRecords = await query.CountAsync();

        // Aplicar paginación
        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.Name)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(u => new ClientResponseDTO
            {
                Id = u.Id,
                Ci = u.Ci,
                Name = u.Name,
                LastName = u.LastName,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return new PagedResponse<ClientResponseDTO>
        {
            Data = users,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)queryParams.PageSize)
        };
    }

    public async Task<ClientResponseDTO> GetUserAsync(string userId)
    {
        var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return null;
        return new ClientResponseDTO
        {
            Id = user.Id,
            Ci = user.Ci,
            Name = user.Name,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt
        };
    }
    public async Task<PagedResponse<ReservationResponseDTO>> GetUserWithLoansAsync(string userId, ClientReservationQueryParamsDTO queryParams)
    {
        var query = _context.Reservations
            .Include(r => r.Employee)
            .Include(r => r.Book)
            .Where(r => r.ClientId == userId)
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrEmpty(queryParams.Status))
            query = query.Where(r => r.Status == queryParams.Status);

        if (!string.IsNullOrEmpty(queryParams.EmployeeName))
            query = query.Where(r =>
                r.Employee.Name.Contains(queryParams.EmployeeName) ||
                r.Employee.LastName.Contains(queryParams.EmployeeName));

        // Contar total antes de paginar
        var totalRecords = await query.CountAsync();

        var reservations = await query
         .OrderByDescending(r => r.StartDate)
         .Skip((queryParams.Page - 1) * queryParams.PageSize)
         .Take(queryParams.PageSize)
         .Select(r => new ReservationResponseDTO
         {
             Id = r.Id,
             Status = r.Status,
             StartDate = r.StartDate,
             EndDate = r.EndDate,
             CreatedAt=r.CreatedAt,
             UpdatedAt=r.UpdatedAt,
             Book = new ResponseBookDTO
             {
                 Id = r.Book!.Id,
                 Name = r.Book.Name,
                 ISBN = r.Book.ISBN,
                 Author = r.Book.Author,
                 YearPublished = r.Book.YearPublished,
                 Editorial = r.Book.Editorial,
                 CreatedAt=r.Book.CreatedAt,
                 UpdatedAt=r.Book.UpdatedAt,
             },
             Employee = new EmployeeResponseDTO
             {
                 Id = r.Employee.Id,
                 Name = r.Employee.Name,
                 LastName = r.Employee.LastName,
                 Ci = r.Employee.Ci,
                 Email = r.Employee.Email,
                 CreatedAt = r.Employee.CreatedAt,
                 UpdatedAt = r.Employee.UpdatedAt,
             }
         })
         .ToListAsync();
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
