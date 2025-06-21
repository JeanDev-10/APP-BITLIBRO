using System;
using System.ComponentModel.DataAnnotations;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs;
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

public class EmployeeService : IEmployeeService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;

    public EmployeeService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<PagedResponse<EmployeeResponseDTO>> GetAllEmployeesAsync(EmployeeQueryParams queryParams)
    {
        // Primero obtenemos todos los IDs de empleados
        var employeeIds = await _userManager.GetUsersInRoleAsync("Employee");
        var employeeIdsList = employeeIds.Select(u => u.Id).ToList();

        // Construimos la consulta base
        var query = _userManager.Users
            .Where(u => employeeIdsList.Contains(u.Id))
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrEmpty(queryParams.Name))
            query = query.Where(u => u.Name.Contains(queryParams.Name));

        if (!string.IsNullOrEmpty(queryParams.LastName))
            query = query.Where(u => u.LastName.Contains(queryParams.LastName));

        if (!string.IsNullOrEmpty(queryParams.Ci))
            query = query.Where(u => u.Ci.Contains(queryParams.Ci));

        // Contar total antes de paginar
        var totalRecords = await query.CountAsync();

        // Aplicar paginación
        var employees = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.Name)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(u => new EmployeeResponseDTO
            {
                Id = u.Id,
                Email = u.Email!,
                Name = u.Name,
                LastName = u.LastName,
                Ci = u.Ci,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync();

        return new PagedResponse<EmployeeResponseDTO>
        {
            Data = employees,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)queryParams.PageSize)
        };
    }
    public async Task<EmployeeResponseDTO> GetEmployeeByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null || !(await _userManager.IsInRoleAsync(user, "Employee")))
            return null;

        return new EmployeeResponseDTO
        {
            Id = user.Id,
            Email = user.Email!,
            Name = user.Name,
            LastName = user.LastName,
            Ci = user.Ci,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public async Task<EmployeeResponseDTO> CreateEmployeeAsync(CreateEmployeeDTO createDto)
    {
        var user = new User
        {
            UserName = createDto.Email,
            Email = createDto.Email,
            Name = createDto.Name,
            LastName = createDto.LastName,
            Ci = createDto.Ci,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, createDto.Password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        // Asignar rol de Employee
        await _userManager.AddToRoleAsync(user, "Employee");

        return new EmployeeResponseDTO
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            LastName = user.LastName,
            Ci = user.Ci,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public async Task<PagedResponse<ReservationResponseDTO>> GetReservationsByEmployeeAsync(
    string employeeId, EmployeeReservationQueryParamsDTO queryParams)
    {
        // Verificar que el empleado existe
        var employeeExists = await _userManager.Users.AnyAsync(u => u.Id == employeeId);
        if (!employeeExists)
            throw new KeyNotFoundException("Empleado no encontrado");

        var query = _context.Reservations
            .Include(r => r.Client)
            .Include(r => r.Book)
                .ThenInclude(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
            .Include(r => r.Book)
                .ThenInclude(b => b.Images)
            .Where(r => r.EmployeeId == employeeId)
            .AsQueryable();
        // Aplicar filtros
        if (!string.IsNullOrEmpty(queryParams.Status))
            query = query.Where(r => r.Status == queryParams.Status);

        if (!string.IsNullOrEmpty(queryParams.ClientName))
            query = query.Where(r =>
                r.Client.Name.Contains(queryParams.ClientName) ||
                r.Client.LastName.Contains(queryParams.ClientName));

        // Contar total antes de paginar
        var totalRecords = await query.CountAsync();
        // Aplicar paginación
        var reservations = await query
            .OrderByDescending(r => r.StartDate)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(reservation => new ReservationResponseDTO
            {
                Id = reservation.Id,
                Status = reservation.Status,
                Book = new ResponseBookDTO
                {
                    Id = reservation.Book!.Id,
                    Name = reservation.Book.Name,
                    ISBN = reservation.Book.ISBN,
                    YearPublished = reservation.Book.YearPublished,
                    Editorial = reservation.Book.Editorial,
                    Author = reservation.Book.Author,
                    CreatedAt = reservation.Book.CreatedAt,
                    UpdatedAt = reservation.Book.UpdatedAt,
                    Genres = reservation.Book.BookGenres!.Select(bg => new GenreResponseDto
                    {
                        Id = bg.Genre!.Id,
                        Name = bg.Genre.Name,
                        CreatedAt = bg.Genre.CreatedAt,
                        UpdatedAt = bg.Genre.UpdatedAt
                    }).ToList(),
                    ImageUrls = reservation.Book.Images!.Select(i => new ResponseImageDTO
                    {
                        Id = i.Id,
                        Url = i.Url
                    }).ToList()
                },
                Client = new ClientResponseDTO
                {
                    Id = reservation.Client!.Id,
                    Name = reservation.Client!.Name,
                    LastName = reservation.Client.LastName,
                    Ci = reservation.Client.Ci,
                    CreatedAt = reservation.Client.CreatedAt
                },
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
                CreatedAt = reservation.CreatedAt,
                UpdatedAt = reservation.UpdatedAt
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
    public async Task<EmployeeResponseDTO> UpdateEmployeeAsync(UpdateEmployeeDTO updateDto)
    {
        var user = await _userManager.FindByIdAsync(updateDto.Id);
        if (user == null || !(await _userManager.IsInRoleAsync(user, "Employee")))
            return null;

        user.Name = updateDto.Name;
        user.LastName = updateDto.LastName;
        user.Ci = updateDto.Ci;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        return new EmployeeResponseDTO
        {
            Id = user.Id,
            Email = user.Email!,
            Name = user.Name,
            LastName = user.LastName,
            Ci = user.Ci,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
    public async Task<bool> DeleteEmployeeAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null || !(await _userManager.IsInRoleAsync(user, "Employee")))
            return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

}
