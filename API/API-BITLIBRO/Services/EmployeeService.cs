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
using API_BITLIBRO.Interfaces.Repositories;
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Services;

public class EmployeeService : IEmployeeService
{

    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<PagedResponse<EmployeeResponseDTO>> GetAllEmployeesAsync(EmployeeQueryParams queryParams)
    {
        var employees = await _employeeRepository.GetEmployeesAsync(queryParams);
        var totalRecords = await _employeeRepository.CountEmployeesAsync(queryParams);

        return new PagedResponse<EmployeeResponseDTO>
        {
            Data = employees.Select(u => new EmployeeResponseDTO
            {
                Id = u.Id,
                Email = u.Email!,
                Name = u.Name,
                LastName = u.LastName,
                Ci = u.Ci,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }).ToList(),
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)queryParams.PageSize)
        };
    }
    public async Task<EmployeeResponseDTO> GetEmployeeByIdAsync(string id)
    {
        var user = await _employeeRepository.GetByIdAsync(id);
        if (user == null) return null;
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
        var createdUser = await _employeeRepository.CreateAsync(user, createDto.Password);
        if (createdUser == null)
            throw new Exception("No se pudo crear el empleado");
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
        var exists = await _employeeRepository.ExistsAsync(employeeId);
        if (!exists)
            throw new KeyNotFoundException("Empleado no encontrado");

        var reservations = await _employeeRepository.GetReservationsByEmployeeAsync(employeeId, queryParams);
        var totalRecords = reservations.Count; // ya está paginado
        return new PagedResponse<ReservationResponseDTO>
        {
            Data = reservations.Select(r => new ReservationResponseDTO
            {
                Id = r.Id,
                Status = r.Status,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Client = new ClientResponseDTO
                {
                    Id = r.Client!.Id,
                    Name = r.Client.Name,
                    LastName = r.Client.LastName,
                    Ci = r.Client.Ci,
                    CreatedAt = r.Client.CreatedAt
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
                }
            }).ToList(),
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)queryParams.PageSize)
        };
    }
    public async Task<EmployeeResponseDTO> UpdateEmployeeAsync(UpdateEmployeeDTO updateDto)
    {
        var user = await _employeeRepository.GetByIdAsync(updateDto.Id);
        if (user == null) return null;

        user.Name = updateDto.Name;
        user.LastName = updateDto.LastName;
        user.Ci = updateDto.Ci;
        user.UpdatedAt = DateTime.UtcNow;

        var updated = await _employeeRepository.UpdateAsync(user);
        if (!updated)
            throw new Exception("Error al actualizar empleado");

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
        var user = await _employeeRepository.GetByIdAsync(id);
        if (user == null) return false;
        return await _employeeRepository.DeleteAsync(user);
    }

}
