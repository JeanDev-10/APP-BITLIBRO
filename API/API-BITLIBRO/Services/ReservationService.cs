using System.Security.Claims;
using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.Reservation;
using API_BITLIBRO.Interfaces;
using API_BITLIBRO.Interfaces.Repositories;
using API_BITLIBRO.Interfaces.Transactions;
using API_BITLIBRO.Mapper;
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Identity;

namespace API_BITLIBRO.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    public ReservationService(IReservationRepository reservationRepository, UserManager<User> userManager, IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }
    public async Task<PagedResponse<ReservationResponseDTO>> GetAllReservationsAsync(ReservationQueryParamsDTO queryParams, ClaimsPrincipal user, bool isAdmin)
    {
        // Si no es admin, limitar por employeeId (el empleado solo ve sus propias reservas)
        string? employeeId = null;
        var filterByEmployee = false;
        if (!isAdmin)
        {
            employeeId = _userManager.GetUserId(user);
            filterByEmployee = true;
        }

        var (items, total) = await _reservationRepository.GetPagedAsync(queryParams, employeeId, filterByEmployee);
        var dtos = items.Select(i => i.ToDto()
        ).ToList();
        return new PagedResponse<ReservationResponseDTO>
        {
            Data = dtos,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling(total / (double)queryParams.PageSize)
        };
    }

    public async Task<ReservationResponseDTO> GetReservationByIdAsync(int id, ClaimsPrincipal user, bool isAdmin)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null) return null;
        if (!isAdmin)
        {
            var employeeId = _userManager.GetUserId(user);
            // seguridad: si no es admin y no es su reserva, no devolver
            if (reservation.EmployeeId != employeeId) return null;
        }
        return reservation.ToDto();
    }
    public async Task<ReservationResponseDTO> CreateReservationAsync(CreateReservationDTO createDto, string employeeId)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1) Validar que el libro exista
            if (!await _reservationRepository.BookExistsAsync(createDto.BookId))
                throw new Exception("El libro no existe");

            // 2) Verificar solapamiento
            var overlapping = await _reservationRepository.ExistsOverlappingReservationAsync(createDto.BookId, createDto.StartDate, createDto.EndDate);
            if (overlapping)
                throw new Exception("El libro ya está reservado en ese período");

            // 3) Obtener/crear cliente
            User user;
            if (!string.IsNullOrEmpty(createDto.UserId))
            {
                user = await _userManager.FindByIdAsync(createDto.UserId);
                if (user == null)
                    throw new Exception("Usuario no encontrado");
            }
            else
            {
                user = new User
                {
                    UserName = createDto.Ci,
                    Ci = createDto.Ci!,
                    Name = createDto.Name!,
                    LastName = createDto.LastName!,
                    Email = $"{createDto.Ci}@fake.com",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    throw new Exception($"Error al crear usuario: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

                await _userManager.AddToRoleAsync(user, "Client");
            }

            // 4) Crear reserva
            var reservation = new Reservation
            {
                Status = "Pendiente",
                EmployeeId = employeeId,
                BookId = createDto.BookId,
                ClientId = user.Id,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _reservationRepository.AddAsync(reservation);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitAsync();

            var created = await _reservationRepository.GetByIdAsync(reservation.Id);
            return created!.ToDto();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<ReservationResponseDTO> UpdateReservationAsync(UpdateReservationDTO updateDto, string employeeId, bool isAdmin)
    {
        // 1) Obtener la entidad tracked (porque la vamos a modificar)
        var reservation = await _reservationRepository.FindTrackedByIdAsync(updateDto.Id);
        if (reservation == null)
            throw new Exception("Reserva no encontrada");

        // 2) Permisos: solo admin o empleado dueño puede editar
        if (!isAdmin && reservation.EmployeeId != employeeId)
            throw new Exception("No tienes permiso para editar esta reserva");

        // 3) Si se cambia el libro, validar existencia y disponibilidad
        if (updateDto.BookId != 0 && updateDto.BookId != reservation.BookId)
        {
            if (!await _reservationRepository.BookExistsAsync(updateDto.BookId))
                throw new Exception("El nuevo libro no existe");

            var overlapping = await _reservationRepository.ExistsOverlappingReservationAsync(
                updateDto.BookId,
                updateDto.StartDate,
                updateDto.EndDate,
                excludeReservationId: updateDto.Id);

            if (overlapping)
                throw new Exception("El nuevo libro ya está reservado en ese período");

            reservation.BookId = updateDto.BookId;
        }

        // 4) Actualizar campos permitidos
        if (!string.IsNullOrEmpty(updateDto.Status))
            reservation.Status = updateDto.Status;

        reservation.EndDate = updateDto.EndDate;
        reservation.UpdatedAt = DateTime.UtcNow;

        // 5) Guardar cambios
        await _reservationRepository.UpdateAsync(reservation);
        await _reservationRepository.SaveChangesAsync();

        var updated = await _reservationRepository.GetByIdAsync(reservation.Id);
        return updated!.ToDto();
    }
}
