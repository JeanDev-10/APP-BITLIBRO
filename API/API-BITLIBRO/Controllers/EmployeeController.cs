using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.ApiResponse;
using API_BITLIBRO.DTOs.Employee;
using API_BITLIBRO.DTOs.Reservation;
using API_BITLIBRO.Interfaces;
using API_BITLIBRO.Models;
using API_BITLIBRO.Services;
using API_BITLIBRO.Validators.Employee;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API_BITLIBRO.Controllers
{
    [Route("api/employees")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IValidator<CreateEmployeeDTO> _createValidator;
        private readonly IValidator<UpdateEmployeeDTO> _updateValidator;
        public EmployeeController(
          IEmployeeService employeeService,
          IValidator<CreateEmployeeDTO> createValidator,
          IValidator<UpdateEmployeeDTO> updateValidator
          )
        {
            _employeeService = employeeService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }
        [HttpGet]
        public async Task<ActionResult<ApiResponseData<PagedResponse<EmployeeResponseDTO>>>> GetAll([FromQuery] EmployeeQueryParams queryParams)
        {
            var result = await _employeeService.GetAllEmployeesAsync(queryParams);
            return Ok(ApiResponseData<PagedResponse<EmployeeResponseDTO>>.Success(result, "Empleados obtenidos correctamente"));
        }
        [HttpGet("{id}/reservations")]
        public async Task<ActionResult<ApiResponseData<PagedResponse<ReservationResponseDTO>>>> GetByIdWithReservations(string id, [FromQuery] EmployeeReservationQueryParamsDTO queryParams)
        {
            try
            {

                var reservations = await _employeeService.GetReservationsByEmployeeAsync(id, queryParams);
                if (reservations == null) return NotFound(ApiResponse.Fail("Empleado no encontrado"));
                return Ok(ApiResponseData<PagedResponse<ReservationResponseDTO>>.Success(reservations, "Reservaciones obtenidas"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse.Fail(
                            ex.Message
                        ));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail(
                                           ex.Message
                                       ));
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseData<EmployeeResponseDTO>>> GetById(string id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound(ApiResponse.Fail("Empleado no encontrado"));
            return Ok(ApiResponseData<EmployeeResponseDTO>.Success(employee, "Empleado obtenido correctamente"));
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDTO createDto)
        {

            // Validación manual asíncrona
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseData<List<string>>.Fail("Errores de validación", errores));
            }
            try
            {
                var newEmployee = await _employeeService.CreateEmployeeAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = newEmployee.Id }, newEmployee);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseData<EmployeeResponseDTO>>> Update(string id, [FromBody] UpdateEmployeeDTO updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest(ApiResponse.Fail("El ID del empleado no coincide con el ID en el cuerpo de la solicitud."));
            }
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseData<List<string>>.Fail("Errores de validación", errores));
            }

            try
            {
                var updatedEmployee = await _employeeService.UpdateEmployeeAsync(updateDto);
                if (updatedEmployee == null) return NotFound(ApiResponse.Fail("Empleado no encontrado"));
                return Ok(ApiResponseData<EmployeeResponseDTO>.Success(updatedEmployee, "Empleado actualizado correctamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _employeeService.DeleteEmployeeAsync(id);
            if (!result) return NotFound(ApiResponse.Fail("Empleado no encontrado"));

            return NoContent();
        }
    }
}
