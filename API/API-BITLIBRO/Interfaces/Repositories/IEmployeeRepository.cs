using System;
using API_BITLIBRO.DTOs.Employee;
using API_BITLIBRO.Models;

namespace API_BITLIBRO.Interfaces.Repositories;

public interface IEmployeeRepository
{
    Task<IList<User>> GetEmployeesAsync(EmployeeQueryParams queryParams);
    Task<int> CountEmployeesAsync(EmployeeQueryParams queryParams);
    Task<User?> GetByIdAsync(string id);
    Task<User?> CreateAsync(User user, string password);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(User user);
    Task<bool> ExistsAsync(string id);
    Task<IList<Reservation>> GetReservationsByEmployeeAsync(string employeeId, EmployeeReservationQueryParamsDTO queryParams);
}
