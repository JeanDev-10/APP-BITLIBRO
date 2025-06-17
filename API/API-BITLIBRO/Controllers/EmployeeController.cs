using API_BITLIBRO.DTOs.Employee;
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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService _employeeService;
        private readonly IValidator<CreateEmployeeDTO> _createValidator;
        private readonly IValidator<UpdateEmployeeDTO> _updateValidator;
        public EmployeeController(
          EmployeeService employeeService,
          IValidator<CreateEmployeeDTO> createValidator,
          IValidator<UpdateEmployeeDTO> updateValidator
          )
        {
            _employeeService = employeeService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] EmployeeQueryParams queryParams)
        {
            var result = await _employeeService.GetAllEmployeesAsync(queryParams);
            return Ok(new
            {
                message = "Empleados obtenidos correctamente",
                data = result
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound();

            return Ok(new
            {
                message = "Empleado obtenido correctamente",
                data = employee
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDTO createDto)
        {

            // Validación manual asíncrona
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Errors = errores });
            }
            try
            {
                var newEmployee = await _employeeService.CreateEmployeeAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = newEmployee.Id }, newEmployee);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateEmployeeDTO updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest("El ID del empleado no coincide con el ID en el cuerpo de la solicitud.");
            }
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Errors = errores });
            }

            try
            {
                var updatedEmployee = await _employeeService.UpdateEmployeeAsync(updateDto);
                if (updatedEmployee == null) return NotFound();
                return Ok(updatedEmployee);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _employeeService.DeleteEmployeeAsync(id);
            if (!result) return NotFound();

            return NoContent();
        }
    }
}
