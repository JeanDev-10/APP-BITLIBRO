using API_BITLIBRO.DTOs.Reservation;
using API_BITLIBRO.Models;

namespace API_BITLIBRO.Interfaces.Repositories;

public interface IReservationRepository
{
    Task<(IReadOnlyList<Reservation> Items, int Total)> GetPagedAsync(ReservationQueryParamsDTO queryParams,string? employeeId = null, bool filterByEmployee = false);
    Task<Reservation?> GetByIdAsync(int id);
    Task<Reservation?> FindTrackedByIdAsync(int id);
    Task<bool> BookExistsAsync(int bookId);
    Task<bool> ExistsOverlappingReservationAsync(int bookId, DateTime startDate, DateTime endDate, int? excludeReservationId = null);

    Task AddAsync(Reservation reservation);
    Task UpdateAsync(Reservation reservation);
    Task DeleteAsync(Reservation reservation);
    Task<int> SaveChangesAsync();

}
