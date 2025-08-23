using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs.Employee;
using API_BITLIBRO.Interfaces.Repositories;
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;

    public EmployeeRepository(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }
    public async Task<IList<User>> GetEmployeesAsync(EmployeeQueryParams queryParams)
    {
        var employees = await _userManager.GetUsersInRoleAsync("Employee");
        var employeeIds = employees.Select(u => u.Id).ToList();

        var query = _userManager.Users
            .Where(u => employeeIds.Contains(u.Id))
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Name))
            query = query.Where(u => u.Name.Contains(queryParams.Name));

        if (!string.IsNullOrEmpty(queryParams.LastName))
            query = query.Where(u => u.LastName.Contains(queryParams.LastName));

        if (!string.IsNullOrEmpty(queryParams.Ci))
            query = query.Where(u => u.Ci.Contains(queryParams.Ci));

        return await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.Name)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();
    }
    public async Task<int> CountEmployeesAsync(EmployeeQueryParams queryParams)
    {
        var employees = await _userManager.GetUsersInRoleAsync("Employee");
        var employeeIds = employees.Select(u => u.Id).ToList();

        var query = _userManager.Users
            .Where(u => employeeIds.Contains(u.Id))
            .AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Name))
            query = query.Where(u => u.Name.Contains(queryParams.Name));

        if (!string.IsNullOrEmpty(queryParams.LastName))
            query = query.Where(u => u.LastName.Contains(queryParams.LastName));

        if (!string.IsNullOrEmpty(queryParams.Ci))
            query = query.Where(u => u.Ci.Contains(queryParams.Ci));

        return await query.CountAsync();
    }
    public async Task<User?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null && await _userManager.IsInRoleAsync(user, "Employee"))
            return user;

        return null;
    }
    public async Task<User?> CreateAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return null;

        await _userManager.AddToRoleAsync(user, "Employee");
        return user;
    }
    public async Task<bool> UpdateAsync(User user)
    {
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
    public async Task<bool> DeleteAsync(User user)
    {
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
    public async Task<bool> ExistsAsync(string id)
    {
        return await _userManager.Users.AnyAsync(u => u.Id == id);
    }
     public async Task<IList<Reservation>> GetReservationsByEmployeeAsync(string employeeId, EmployeeReservationQueryParamsDTO queryParams)
        {
            var query = _context.Reservations
                .Include(r => r.Client)
                .Include(r => r.Book)
                    .ThenInclude(b => b.BookGenres)
                        .ThenInclude(bg => bg.Genre)
                .Include(r => r.Book)
                    .ThenInclude(b => b.Images)
                .Where(r => r.EmployeeId == employeeId)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(queryParams.Status))
                query = query.Where(r => r.Status == queryParams.Status);

            if (!string.IsNullOrEmpty(queryParams.ClientName))
                query = query.Where(r =>
                    r.Client.Name.Contains(queryParams.ClientName) ||
                    r.Client.LastName.Contains(queryParams.ClientName));

            if (queryParams.StartDate.HasValue)
                query = query.Where(r => r.StartDate >= queryParams.StartDate.Value);

            if (queryParams.EndDate.HasValue)
                query = query.Where(r => r.StartDate <= queryParams.EndDate.Value);

            return await query
                .OrderByDescending(r => r.StartDate)
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();
        }
}
