using System;
using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.ApiResponse;
using API_BITLIBRO.DTOs.Client;
using API_BITLIBRO.DTOs.Reservation;

namespace API_BITLIBRO.Interfaces;

public interface IClientService
{
    Task<PagedResponse<ClientResponseDTO>> GetUsersAsync(ClientQueryParamsDTO queryParams);
    Task<PagedResponse<ReservationResponseDTO>> GetUserWithLoansAsync(string userId,ClientReservationQueryParamsDTO queryParams);
    Task<ClientResponseDTO> GetUserAsync(string userId);
}
