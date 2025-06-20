using System;
using System.Security.Claims;
using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.Reservation;

namespace API_BITLIBRO.Interfaces;

public interface IReservationService
{
    public Task<PagedResponse<ReservationResponseDTO>> GetAllReservationsAsync(ReservationQueryParamsDTO queryParams, ClaimsPrincipal user, bool isAdmin);
    public Task<ReservationResponseDTO> GetReservationByIdAsync(int id, ClaimsPrincipal user, bool isAdmin);
    public Task<ReservationResponseDTO> CreateReservationAsync(CreateReservationDTO createDto, string employeeId);
    public Task<ReservationResponseDTO> UpdateReservationAsync(UpdateReservationDTO updateDto, string employeeId, bool isAdmin);

}
