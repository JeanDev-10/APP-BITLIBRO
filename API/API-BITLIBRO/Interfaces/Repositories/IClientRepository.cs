using API_BITLIBRO.DTOs.Client;
using API_BITLIBRO.Models;

namespace API_BITLIBRO.Interfaces.Repositories;

public interface IClientRepository
{
    Task<(List<User> Items, int Total)> GetPagedClientsAsync(ClientQueryParamsDTO clientQueryParamsDTO);
    Task<User?> GetClientByIdAsync(string userId, bool ensureClientRole = true);
    Task<(List<Reservation> Items, int Total)> GetClientReservationsPagedAsync(string userId, ClientReservationQueryParamsDTO queryParams);
}

