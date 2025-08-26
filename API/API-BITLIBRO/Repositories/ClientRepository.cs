using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs.Client;
using API_BITLIBRO.Interfaces.Repositories;
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public ClientRepository(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public async Task<(List<User> Items, int Total)> GetPagedClientsAsync(ClientQueryParamsDTO queryParams)
    {
        var clientUsers = await _userManager.GetUsersInRoleAsync("Client");
        var clientIds = clientUsers.Select(u => u.Id).ToList();

        var query = _userManager.Users
            .Where(u => clientIds.Contains(u.Id))
            .AsNoTracking()
            .AsQueryable();
        // Filtros
        if (!string.IsNullOrEmpty(queryParams.Ci))
            query = query.Where(u => u.Ci.Contains(queryParams.Ci));

        if (!string.IsNullOrEmpty(queryParams.Name))
            query = query.Where(u => u.Name.Contains(queryParams.Name));

        if (!string.IsNullOrEmpty(queryParams.LastName))
            query = query.Where(u => u.LastName.Contains(queryParams.LastName));

        // Total
        var total = await query.CountAsync();

        var items = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.Name)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();
        return (items, total);
    }
    public async Task<User?> GetClientByIdAsync(string userId, bool ensureClientRole = true)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;

        if (!ensureClientRole)
            return user;
        var inRole = await _userManager.IsInRoleAsync(user, "Client");
        return inRole ? user : null;
    }
    public async Task<(List<Reservation> Items, int Total)> GetClientReservationsPagedAsync(
         string userId, ClientReservationQueryParamsDTO queryParams)
    {
        var query = _context.Reservations
                 .AsNoTracking()
                 .Include(r => r.Employee)
                 .Include(r => r.Book).ThenInclude(b => b.BookGenres).ThenInclude(bg => bg.Genre)
                 .Include(r => r.Book).ThenInclude(b => b.Images)
                 .Where(r => r.ClientId == userId)
                 .AsQueryable();

        // Filtros
        if (!string.IsNullOrEmpty(queryParams.Status))
            query = query.Where(r => r.Status == queryParams.Status);

        if (!string.IsNullOrEmpty(queryParams.EmployeeName))
            query = query.Where(r =>
                r.Employee!.Name.Contains(queryParams.EmployeeName) ||
                r.Employee!.LastName.Contains(queryParams.EmployeeName));

        if (queryParams.StartDate.HasValue && queryParams.EndDate.HasValue)
        {
            query = query.Where(r =>
                r.StartDate >= queryParams.StartDate.Value &&
                r.StartDate <= queryParams.EndDate.Value);
        }
        else if (queryParams.StartDate.HasValue)
        {
            query = query.Where(r => r.StartDate >= queryParams.StartDate.Value);
        }
        else if (queryParams.EndDate.HasValue)
        {
            query = query.Where(r => r.StartDate <= queryParams.EndDate.Value);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.StartDate)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return (items, total);
    }
}
