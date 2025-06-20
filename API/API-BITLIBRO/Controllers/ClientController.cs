using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.ApiResponse;
using API_BITLIBRO.DTOs.Client;
using API_BITLIBRO.DTOs.Reservation;
using API_BITLIBRO.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_BITLIBRO.Controllers
{
    [Route("api/clients")]
    [ApiController]
    [Authorize]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }
        [HttpGet]
        public async Task<ActionResult<ApiResponseData<PagedResponse<ClientResponseDTO>>>> GetUsers(
          [FromQuery] ClientQueryParamsDTO queryParams)
        {
            var result = await _clientService.GetUsersAsync(queryParams);
            return Ok(ApiResponseData<PagedResponse<ClientResponseDTO>>.Success(
                result,
                "Clientes obtenidos correctamente"
            ));
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResponseData<ClientResponseDTO>>> GetUser(string userId)
        {
            var result = await _clientService.GetUserAsync(userId);

            if (result == null)
                return NotFound(ApiResponse.Fail("Cliente no encontrado"));
            return Ok(ApiResponseData<ClientResponseDTO>.Success(result,"Cliente obtenido correctamente"));
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{userId}/reservations")]
        public async Task<ActionResult<ApiResponseData<PagedResponse<ReservationResponseDTO>>>> GetUserWithLoans(string userId,[FromQuery] ClientReservationQueryParamsDTO queryParams)
        {
            var result = await _clientService.GetUserWithLoansAsync(userId,queryParams);

            if (result == null)
                return NotFound(ApiResponse.Fail("Cliente no encontrado"));
            return Ok(ApiResponseData<PagedResponse<ReservationResponseDTO>>.Success(result,"Cliente obtenido correctamente"));
        }
    }
}
