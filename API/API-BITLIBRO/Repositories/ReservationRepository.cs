using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs.Reservation;
using API_BITLIBRO.Interfaces.Repositories;
using API_BITLIBRO.Models;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _context;

    public ReservationRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<(IReadOnlyList<Reservation> Items, int Total)> GetPagedAsync(
            ReservationQueryParamsDTO queryParams,
            string? employeeId = null,
            bool filterByEmployee = false)
    {
        var query = _context.Reservations
            .Include(r => r.Employee)
            .Include(r => r.Client)
            .Include(r => r.Book)
                .ThenInclude(b => b.Images)
            .Include(r => r.Book)
                .ThenInclude(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
            .AsNoTracking()
            .AsQueryable();
        if (!string.IsNullOrEmpty(queryParams.Status))
            query = query.Where(r => r.Status == queryParams.Status);

        if (!string.IsNullOrEmpty(queryParams.ClientName))
            query = query.Where(r =>
                r.Client != null &&
                (r.Client.Name.Contains(queryParams.ClientName) ||
                 r.Client.LastName.Contains(queryParams.ClientName)));

        if (!string.IsNullOrEmpty(queryParams.EmployeeName))
            query = query.Where(r =>
                r.Employee != null &&
                (r.Employee.Name.Contains(queryParams.EmployeeName) ||
                 r.Employee.LastName.Contains(queryParams.EmployeeName)));

        // Si se debe filtrar por empleado (empleado normal), aplicar
        if (filterByEmployee && !string.IsNullOrEmpty(employeeId))
            query = query.Where(r => r.EmployeeId == employeeId);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return (items, total);
    }
    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _context.Reservations
            .Include(r => r.Employee)
            .Include(r => r.Client)
            .Include(r => r.Book)
                .ThenInclude(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
            .Include(r => r.Book)
                .ThenInclude(b => b.Images)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }
    public async Task<Reservation?> FindTrackedByIdAsync(int id)
    {
        return await _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Client)
            .Include(r => r.Employee)
            .Include(r => r.Book)
                .ThenInclude(b => b.Images)
            .Include(r => r.Book)
                .ThenInclude(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
    public async Task<bool> BookExistsAsync(int bookId)
    {
        return await _context.Books.AsNoTracking().AnyAsync(b => b.Id == bookId);
    }
    public async Task<bool> ExistsOverlappingReservationAsync(int bookId, DateTime startDate, DateTime endDate, int? excludeReservationId = null)
    {
        var q = _context.Reservations
            .Where(r => r.BookId == bookId && r.Status == "Pendiente");

        if (excludeReservationId.HasValue)
            q = q.Where(r => r.Id != excludeReservationId.Value);

        return await q.AnyAsync(r => r.StartDate <= endDate && r.EndDate >= startDate);
    }

    public async Task AddAsync(Reservation reservation)
    {
        await _context.Reservations.AddAsync(reservation);
    }

    public Task UpdateAsync(Reservation reservation)
    {
        _context.Reservations.Update(reservation);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Reservation reservation)
    {
        _context.Reservations.Remove(reservation);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
