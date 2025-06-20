using System.Security.Claims;
using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.ApiResponse;
using API_BITLIBRO.DTOs.Reservation;
using API_BITLIBRO.Interfaces;
using API_BITLIBRO.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_BITLIBRO.Controllers
{
    [Route("api/reservations")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly IValidator<CreateReservationDTO> _createValidator;
        private readonly IValidator<UpdateReservationDTO> _updateValidator;

        public ReservationController(
            IReservationService reservationService,
            IValidator<CreateReservationDTO> createValidator,
            IValidator<UpdateReservationDTO> updateValidator)
        {
            _reservationService = reservationService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]

        public async Task<ActionResult<ApiResponseData<PagedResponse<ReservationResponseDTO>>>> GetAll([FromQuery] ReservationQueryParamsDTO queryParams)
        {
            var isAdmin = User.IsInRole("Admin");
            var result = await _reservationService.GetAllReservationsAsync(queryParams, User, isAdmin);
            return Ok(ApiResponseData<PagedResponse<ReservationResponseDTO>>.Success(result, "Reservas obtenidas correctamente"));
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<ApiResponseData<ReservationResponseDTO>>> GetById(int id)
        {
            var isAdmin = User.IsInRole("Admin");
            var reservation = await _reservationService.GetReservationByIdAsync(id, User, isAdmin);
            if (reservation == null) return NotFound(ApiResponse.Fail("Reserva no encontrada"));
            return Ok(ApiResponseData<ReservationResponseDTO>.Success(reservation, "Reserva obtenida correctamente"));
        }
        [HttpPost]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Create([FromBody] CreateReservationDTO createDto)
        {
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseData<List<string>>.Fail("Errores de validación", errores));
            }

            try
            {
                var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var newReservation = await _reservationService.CreateReservationAsync(createDto, employeeId!);
                return CreatedAtAction(nameof(GetById), new { id = newReservation.Id },newReservation);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<ActionResult<ApiResponseData<ReservationResponseDTO>>> Update(int id, [FromBody] UpdateReservationDTO updateDto)
        {
            if (updateDto.Id != id)
                return BadRequest("El ID de la reserva no coincide con el ID en la URL");

            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseData<List<string>>.Fail("Errores de validación", errores));

            }

            try
            {
                var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                var updatedReservation = await _reservationService.UpdateReservationAsync(updateDto, employeeId!, isAdmin);
                return Ok(ApiResponseData<ReservationResponseDTO>.Success(updatedReservation, "Reserva actualizada correctamente"));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));

            }
        }
    }
}
