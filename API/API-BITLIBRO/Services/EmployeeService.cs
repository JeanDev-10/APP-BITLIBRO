using System;
using System.ComponentModel.DataAnnotations;
using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.Employee;
using API_BITLIBRO.Interfaces;
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Services;

public class EmployeeService:IEmployeeService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public EmployeeService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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
