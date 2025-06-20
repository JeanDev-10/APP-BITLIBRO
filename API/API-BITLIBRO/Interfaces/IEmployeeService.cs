using System;
using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.Employee;

namespace API_BITLIBRO.Interfaces;

public interface IEmployeeService
{
    public Task<PagedResponse<EmployeeResponseDTO>> GetAllEmployeesAsync(EmployeeQueryParams queryParams);
    public Task<EmployeeResponseDTO> GetEmployeeByIdAsync(string id);
    public Task<EmployeeResponseDTO> CreateEmployeeAsync(CreateEmployeeDTO createDto);
    public Task<EmployeeResponseDTO> UpdateEmployeeAsync(UpdateEmployeeDTO updateDto);
    public Task<bool> DeleteEmployeeAsync(string id);

}
